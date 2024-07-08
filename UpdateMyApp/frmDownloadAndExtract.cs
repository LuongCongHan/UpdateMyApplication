using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;

namespace UpdateMyApp
{
    public partial class frmDownloadAndExtract : Form
    {
        public frmDownloadAndExtract()
        {
            InitializeComponent();
            this.MaximizeBox = false;//Ngăn không cho phóng to form khi nhấn đúp vào thanh điêu đề ( title bar)
            methodInvoker = new MethodInvoker(() => {
                progressBar1.Value = progressbarValue;
                lbPercent.Text = string.Format("{0} %", (int)((float)progressbarValue / int.MaxValue *100)); //Hiển thị phần trăm nguyên
            });
            progressBar1.Maximum = int.MaxValue;
        }
        public UpdateVersion _updateVersion;
        string _downloadZipPath;
        public FileInfo _fileInfo;
        public long? _toTalLengthUnzip = 0;
        int progressbarValue = 0;


        private async Task<bool> DownloadNewVersion(string path, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                Directory.CreateDirectory(path);// Tạo thư mục lưu dữ liệu
                Task taskRefreshFile;
                using (HttpClient _httpClient = new HttpClient())
                using (var stream = await _httpClient.GetStreamAsync(_updateVersion.Url))
                {
                    //Tạo đường link thư mục  temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}/download.zip
                    _downloadZipPath = Path.Combine(path, Path.GetFileName(_updateVersion.Url));
                    using (var fileStream = File.Create(_downloadZipPath)) //Tạo file download.zip
                    {
                        _fileInfo = new FileInfo(_downloadZipPath);
                        //File đã được tạo ở trên nên không lỗi được ở AutoRefresh => File.Create(_downloadZipPath))
                        taskRefreshFile = AutoRefreshAsync(_fileInfo, _toTalLengthUnzip);
                        await stream.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                    }
                }
                //Đợi đến khi thanh hiển thị đủ 100%
                await Task.WhenAny(taskRefreshFile);
                return true;
            }
            catch (TaskCanceledException)
            {
                //Huỷ quá trình tải xuống
                return false;
            }
            catch (Exception ex)
            {
                //Nếu xảy ra lỗi trong quá trình tải
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        long _lengthTemp = 0;
        MethodInvoker methodInvoker;
        long _currentIndexLength = 0;
        bool isStopRefresh = false;
        private async Task AutoRefreshAsync(FileInfo file, long? value)
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    file.Refresh();//Làm mới file
                    try
                    {
                        _lengthTemp = _fileInfo.Length + _currentIndexLength;
                        progressbarValue = (int)(_lengthTemp * int.MaxValue / _toTalLengthUnzip);
                        this.Invoke(methodInvoker); //Xuất thông báo ra Progressbar 
                        if (_lengthTemp == value || isStopRefresh) break;
                    }
                    catch { }
                }
            });
        }
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        bool Cancelclosed = false; //Cho phép huỷ khi nhấn close
        public bool isStartNewUpdate = false;
        private async void frmDownloadAndExtract_Load(object sender, EventArgs e)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();//Phải tạo mới nếu như lần trước đó huỷ thì khi mở lại thì các task không hoàn thành

            isStopRefresh = false; //Đặt lại giá trị cho hàm Autofresh
            isStartNewUpdate = false; //Thông báo là đang cập nhật phiên bản
            Cancelclosed = true; //Không cho phép đóng form

            label1.Text = "Downloading...";
            progressBar1.Value = 0;
            _currentIndexLength = 0; //Đặt lại số tệp tin trong file

            Guid guid = GetGuid(Assembly.GetExecutingAssembly());
            string path = Path.Combine(Path.GetTempPath(), "{" + guid.ToString().ToUpper() + "}"); //Tạo path: temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}

            //Tiến hành tải và đợi xong
            //Đợi download xong
            bool isDownloaded = await DownloadNewVersion(path, _cancellationTokenSource);
            _currentIndexLength = 0; //Đặt lại số tệp tin trong file
           

            if (isDownloaded) //Nếu tải thành công
            {
                //Giải nén file
                _toTalLengthUnzip = 0;//Đặt lại dung lượng tổng trước khi extract
                await Task.Delay(500);// Delay 0.5s rồi tiến hành Extract
                label1.Text = "Uncompressing...";

                //Tiến hành Extract
                bool isExtracted = await ExtractAsync(_cancellationTokenSource); //Trong này đã kèm quá trình thanh tiến trình
                if(isExtracted) //Nếu giải nén thành công
                {
                    await Task.Delay(100); //Đợi tý rồi kích hoạt
                    var exeFiles = Directory.EnumerateFiles(pathFolder, "*.exe", SearchOption.TopDirectoryOnly).ToArray();
                    if (exeFiles != null && exeFiles.Length > 0)
                    {
                        Process.Start(exeFiles[0]);
                        Cancelclosed = false; //Cho phép đóng form lại
                        isStartNewUpdate = true; //Bắt đầu chạy version mới
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("The new version file was not found.", "Install");
                    }
                }
                else //Nếu giải nén thất bại
                {
                    Cancelclosed = false; //Cho phép đóng form
                    this.Close();
                }

            }
            else //Nếu tải thất bại
            {
                Cancelclosed = false; //Cho phép đóng form
                this.Close();
            }

        }
        string pathFolder = string.Empty;


        private async Task<bool> ExtractAsync(CancellationTokenSource cancellationTokenSource)
        {
            //Mở file zip để đọc
            using (ZipArchive zip = await Task.Run(() => ZipFile.OpenRead(_downloadZipPath), _cancellationTokenSource.Token))
            {

                //Tạo  thư mục theo tên filezip  temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}/download
                pathFolder = Path.Combine(Path.GetDirectoryName(_downloadZipPath),
                   Path.GetFileNameWithoutExtension(_downloadZipPath));
                Directory.CreateDirectory(pathFolder);

                //Tính dung lượng lưu trữ của các file bên trong file zip (khi unzip xong)
                foreach (ZipArchiveEntry item in zip.Entries)
                {
                    _toTalLengthUnzip += item.Length;
                }
                long? lengthTemp = 0;

                foreach (ZipArchiveEntry item in zip.Entries)
                {
                    Task taskRefreshFile;
                    string path = Path.Combine(pathFolder, item.Name); //Tạo đường dẫn cho từng item  temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}/download/item1.abc
                    _fileInfo = new FileInfo(path); //Tạo file info để theo dõi item1
                    lengthTemp += item.Length; //Dung lượng cộng dồn từng item
                    try
                    {
                        //Tạo file luôn để khỏi lỗi được ở AutoRefresh (hoặc dùng khối using) và dispose nó luôn
                        File.Create(path).Dispose(); //item.abc
                        taskRefreshFile = AutoRefreshAsync(_fileInfo, lengthTemp); //Bắt đầu ghi vào thanh tiến trình

                        //Không thể huỷ ExtractToFile khi nó đang làm việc được=> đợi nó chạy xong mới huỷ được
                        //Bắt đầu giải nén từng item
                        Task taskExtract = Task.Run(() => item.ExtractToFile(path, true), cancellationTokenSource.Token);
                        await Task.WhenAny(taskExtract); //Đợi giải nén xong

                        //Đợi ghi xong thanh extract- Thằng này đang chạy ở trên rồi
                        await Task.WhenAny(taskRefreshFile);

                        //Khi nhấn huỷ
                        if (cancellationTokenSource.IsCancellationRequested)
                            throw new TaskCanceledException();

                        if (taskExtract.Status != TaskStatus.RanToCompletion)
                            throw new Exception();//Khi đang extract mà bị lỗi...
                        _currentIndexLength += item.Length;


                    }
                    catch (TaskCanceledException)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                            //  cancellationTokenSource = new CancellationTokenSource();
                            return false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }
                }
            }
            return true;
        }

        public Guid GetGuid(Assembly assembly)
        {
            var guidAttribute = (GuidAttribute)assembly?.GetCustomAttributes(typeof(GuidAttribute), false).SingleOrDefault();
            if (Guid.TryParse(guidAttribute?.Value, out Guid guid)) { return guid; }
            return Guid.Empty;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Cancel();
            isStopRefresh = true; //Khi nhấn huỷ thì báo cho AutoRefreshAsync đóng lại
            // this.Close(); Không gọi nó ở đây mà đợi các task khác đóng lại hết đã rồi mới Close ở vị trí khác
            //Đợi các task đang thực hiện- Nói cách khác là huỷ các task khác thì ct tự động đóng
        }

        private void frmDownloadAndExtract_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Cancel();
            isStopRefresh = true;
            e.Cancel = Cancelclosed;
        }
    }
}
