using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FolderSize
{
    public partial class Form1 : Form
    {
        // Per-top-level-folder size accumulator (rebuilt on each Render)
        Dictionary<string, long> dicFolderSize = new Dictionary<string, long>();

        // Paths we couldn't read (per-scan, reset each scan)
        List<string> skippedPaths = new List<string>();

        // -----------------------------------------------------------------
        // PERSISTENT CACHE — survives across navigation in/out of folders
        // Key   = full folder path (case-insensitive on Windows)
        // Value = total recursive size in bytes
        // Memory: ~250 bytes per entry. 10,000 folders = ~2.5 MB. Safe.
        // -----------------------------------------------------------------
        Dictionary<string, long> sizeCache =
            new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        decimal gigabyte = 1024m * 1024m * 1024m;
        decimal megabyte = 1024m * 1024m;

        string parentFolder = "";
        bool isScanning = false;

        public Form1()
        {
            InitializeComponent();
            dataGridView1.CellDoubleClick += dataGridView1_CellDoubleClick;
            this.Height = Screen.PrimaryScreen.WorkingArea.Height - 25;
        }

        void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (isScanning) return;
            if (e.RowIndex < 0) return;

            string subfolder = dataGridView1.Rows[e.RowIndex].Cells[colnName.Index].Value + "";
            if (string.IsNullOrEmpty(subfolder) || subfolder == "_root") return;

            string newPath = Path.Combine(parentFolder, subfolder);
            if (!Directory.Exists(newPath)) return;

            parentFolder = newPath;
            StartProcess(forceRescan: false);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isScanning) return;

            FolderBrowserDialog f = new FolderBrowserDialog();
            if (f.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            parentFolder = f.SelectedPath;
            StartProcess(forceRescan: false);
        }

        private void btBack_Click(object sender, EventArgs e)
        {
            if (isScanning) return;

            try
            {
                if (string.IsNullOrEmpty(parentFolder)) return;

                int i = parentFolder.LastIndexOf('\\');
                if (i <= 0) return;

                string newpath = parentFolder.Substring(0, i);
                if (!Directory.Exists(newpath)) return;

                parentFolder = newpath;
                StartProcess(forceRescan: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Force a rescan of the current folder — invalidates cache for it and
        // every cached descendant, then rescans from disk.
        private void btRefresh_Click(object sender, EventArgs e)
        {
            if (isScanning) return;
            if (string.IsNullOrEmpty(parentFolder)) return;

            InvalidateCacheForSubtree(parentFolder);
            StartProcess(forceRescan: true);
        }

        // Wipe the entire cache (rare — for paranoia / reclaim RAM)
        private void btClearCache_Click(object sender, EventArgs e)
        {
            if (isScanning) return;

            int n = sizeCache.Count;
            sizeCache.Clear();
            MessageBox.Show("Cleared " + n + " cached folder size(s).",
                "Cache cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Remove a folder and everything beneath it from the cache.
        // Used when the user hits Refresh — we don't trust cached values for
        // the subtree any more, but we keep entries elsewhere on disk.
        void InvalidateCacheForSubtree(string rootPath)
        {
            string prefix = rootPath.TrimEnd('\\') + "\\";
            List<string> toRemove = new List<string>();

            foreach (string key in sizeCache.Keys)
            {
                if (key.Equals(rootPath, StringComparison.OrdinalIgnoreCase) ||
                    key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    toRemove.Add(key);
                }
            }

            foreach (string k in toRemove)
                sizeCache.Remove(k);
        }

        async void StartProcess(bool forceRescan)
        {
            if (isScanning) return;
            isScanning = true;

            int diskHits = 0;     // how many folders we actually walked from disk
            int cacheHits = 0;    // how many we served from cache

            try
            {
                dataGridView1.Rows.Clear();
                dicFolderSize = new Dictionary<string, long>();
                skippedPaths = new List<string>();

                label1.Text = "Scanning... please wait";
                this.Text = "Scanning: " + parentFolder;
                this.Cursor = Cursors.WaitCursor;

                string scanRoot = parentFolder;

                // Run the scan on a background thread
                await Task.Run(() => ScanFolder(scanRoot, forceRescan,
                                                ref diskHits, ref cacheHits));

                RenderResults(diskHits, cacheHits);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Scan failed: " + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.Text = "Simple Folder Size Viewer  |  Cache: "
                            + sizeCache.Count + " folder(s)";
                isScanning = false;
            }
        }

        // -----------------------------------------------------------------
        // Top-level scan — files in root + each immediate subdir's total
        // -----------------------------------------------------------------
        void ScanFolder(string rootPath, bool forceRescan,
                        ref int diskHits, ref int cacheHits)
        {
            DirectoryInfo rootDi;
            try
            {
                rootDi = new DirectoryInfo(rootPath);
                if (!rootDi.Exists)
                {
                    skippedPaths.Add(rootPath + " | root does not exist");
                    return;
                }
            }
            catch (Exception ex)
            {
                skippedPaths.Add(rootPath + " | " + ex.Message);
                return;
            }

            // Files directly in the root → "_root" bucket. Always read fresh
            // (cheap; one directory's files only) so root-file changes show up.
            long rootSize = SumFilesInDirectory(rootDi);
            dicFolderSize["_root"] = rootSize;

            // Each top-level subdirectory
            IEnumerable<DirectoryInfo> topDirs = SafeEnumerateDirectories(rootDi);
            foreach (DirectoryInfo topDir in topDirs)
            {
                if (IsReparsePoint(topDir))
                {
                    skippedPaths.Add(topDir.FullName + " | skipped (reparse point / junction)");
                    continue;
                }

                long total;
                if (!forceRescan && sizeCache.TryGetValue(topDir.FullName, out total))
                {
                    cacheHits++;
                }
                else
                {
                    total = ComputeFolderSizeRecursive(topDir);
                    sizeCache[topDir.FullName] = total;
                    diskHits++;
                }

                dicFolderSize[topDir.Name] = total;
            }
        }

        // Recursive folder size, with caching at every level.
        // Returns the total size in bytes for `dir` and everything beneath it.
        long ComputeFolderSizeRecursive(DirectoryInfo dir)
        {
            // Cache hit at this level? Done.
            long cached;
            if (sizeCache.TryGetValue(dir.FullName, out cached))
                return cached;

            long total = 0L;

            // Files directly in this folder
            total += SumFilesInDirectory(dir);

            // Subfolders
            foreach (DirectoryInfo sub in SafeEnumerateDirectories(dir))
            {
                if (IsReparsePoint(sub))
                {
                    skippedPaths.Add(sub.FullName + " | skipped (reparse point / junction)");
                    continue;
                }

                try
                {
                    total += ComputeFolderSizeRecursive(sub);
                }
                catch (Exception ex)
                {
                    skippedPaths.Add(sub.FullName + " | " + ex.Message);
                }
            }

            // Cache this folder's total so future visits are instant
            sizeCache[dir.FullName] = total;
            return total;
        }

        // Sum file sizes in ONE directory (non-recursive). Per-file errors
        // are logged, never thrown, so siblings still get counted.
        long SumFilesInDirectory(DirectoryInfo dir)
        {
            long sum = 0L;

            IEnumerable<FileInfo> files = SafeEnumerateFiles(dir);

            using (IEnumerator<FileInfo> it = files.GetEnumerator())
            {
                while (true)
                {
                    FileInfo fi = null;
                    try
                    {
                        if (!it.MoveNext()) break;
                        fi = it.Current;
                    }
                    catch (Exception ex)
                    {
                        skippedPaths.Add(dir.FullName + " | enum fault: " + ex.Message);
                        break;
                    }

                    try
                    {
                        if ((fi.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                        {
                            skippedPaths.Add(fi.FullName + " | skipped (reparse point file)");
                            continue;
                        }

                        sum += fi.Length;
                    }
                    catch (Exception ex)
                    {
                        skippedPaths.Add((fi != null ? fi.FullName : dir.FullName)
                                         + " | " + ex.Message);
                    }
                }
            }

            return sum;
        }

        IEnumerable<DirectoryInfo> SafeEnumerateDirectories(DirectoryInfo dir)
        {
            try
            {
                return dir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
            }
            catch (UnauthorizedAccessException ex)
            {
                skippedPaths.Add(dir.FullName + " | access denied: " + ex.Message);
                return Enumerable.Empty<DirectoryInfo>();
            }
            catch (PathTooLongException ex)
            {
                skippedPaths.Add(dir.FullName + " | path too long: " + ex.Message);
                return Enumerable.Empty<DirectoryInfo>();
            }
            catch (DirectoryNotFoundException ex)
            {
                skippedPaths.Add(dir.FullName + " | not found: " + ex.Message);
                return Enumerable.Empty<DirectoryInfo>();
            }
            catch (IOException ex)
            {
                skippedPaths.Add(dir.FullName + " | IO: " + ex.Message);
                return Enumerable.Empty<DirectoryInfo>();
            }
            catch (Exception ex)
            {
                skippedPaths.Add(dir.FullName + " | " + ex.Message);
                return Enumerable.Empty<DirectoryInfo>();
            }
        }

        IEnumerable<FileInfo> SafeEnumerateFiles(DirectoryInfo dir)
        {
            try
            {
                return dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly);
            }
            catch (UnauthorizedAccessException ex)
            {
                skippedPaths.Add(dir.FullName + " | access denied (files): " + ex.Message);
                return Enumerable.Empty<FileInfo>();
            }
            catch (PathTooLongException ex)
            {
                skippedPaths.Add(dir.FullName + " | path too long (files): " + ex.Message);
                return Enumerable.Empty<FileInfo>();
            }
            catch (DirectoryNotFoundException ex)
            {
                skippedPaths.Add(dir.FullName + " | not found (files): " + ex.Message);
                return Enumerable.Empty<FileInfo>();
            }
            catch (IOException ex)
            {
                skippedPaths.Add(dir.FullName + " | IO (files): " + ex.Message);
                return Enumerable.Empty<FileInfo>();
            }
            catch (Exception ex)
            {
                skippedPaths.Add(dir.FullName + " | " + ex.Message);
                return Enumerable.Empty<FileInfo>();
            }
        }

        bool IsReparsePoint(DirectoryInfo dir)
        {
            try
            {
                return (dir.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
            }
            catch
            {
                return false;
            }
        }

        // -----------------------------------------------------------------
        // UI rendering
        // -----------------------------------------------------------------
        void RenderResults(int diskHits, int cacheHits)
        {
            dataGridView1.Rows.Clear();

            decimal sizeTotal = 0m;

            var sorted = dicFolderSize.OrderByDescending(kv => kv.Value).ToList();

            foreach (KeyValuePair<string, long> kv in sorted)
            {
                DataGridViewRow dgvr = dataGridView1.Rows[dataGridView1.Rows.Add()];

                decimal size = (decimal)kv.Value;
                sizeTotal += size;

                dgvr.Cells[0].Value = kv.Key;
                dgvr.Cells[1].Value = FormatSize(size);
                dgvr.Cells[2].Value = (size / megabyte).ToString("#,##0.00");
            }

            decimal totalGB = sizeTotal / gigabyte;
            decimal totalMB = sizeTotal / megabyte;

            string totalText = "Total: " + totalGB.ToString("#,##0.000") + " GB"
                             + "  (" + totalMB.ToString("#,##0") + " MB)";

            totalText += "  |  Disk: " + diskHits + "  Cache: " + cacheHits;

            if (skippedPaths.Count > 0)
                totalText += "  |  Skipped: " + skippedPaths.Count;

            label1.Text = totalText;

            if (skippedPaths.Count > 0)
            {
                DialogResult r = MessageBox.Show(
                    skippedPaths.Count + " path(s) could not be read and were skipped.\r\n" +
                    "The total above excludes these.\r\n\r\n" +
                    "Save the skipped list to a log file on the Desktop?",
                    "Some paths skipped",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (r == DialogResult.Yes)
                {
                    try
                    {
                        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string logPath = Path.Combine(desktop,
                            "FolderSize_Skipped_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log");
                        File.WriteAllLines(logPath, skippedPaths.ToArray());
                        MessageBox.Show("Saved: " + logPath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not save log: " + ex.Message);
                    }
                }
            }
        }

        string FormatSize(decimal bytes)
        {
            if (bytes >= gigabyte)
                return (bytes / gigabyte).ToString("#,##0.000") + " GB";
            if (bytes >= megabyte)
                return (bytes / megabyte).ToString("#,##0.00") + " MB";
            if (bytes >= 1024m)
                return (bytes / 1024m).ToString("#,##0") + " KB";
            return bytes.ToString("#,##0") + " B";
        }
    }
}
