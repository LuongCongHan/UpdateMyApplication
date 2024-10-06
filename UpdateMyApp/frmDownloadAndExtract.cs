using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;

namespace UpdateMyApp
{
    public partial class frmDownloadAndExtract : Form
    {
        public frmDownloadAndExtract()
        {
            InitializeComponent();
            this.MaximizeBox = false;//Ngăn không cho phóng to form khi nhấn đúp vào thanh điêu đề ( title bar)
            methodInvoker = new MethodInvoker(() =>
            {
                progressBar1.Value = progressbarValue;
                lbPercent.Text = string.Format("{0} %", (int)((float)progressbarValue / int.MaxValue * 100)); //Hiển thị phần trăm nguyên
            });
            progressBar1.Maximum = int.MaxValue;
        }
        public UpdateVersion _updateVersion;
        string _downloadZipPath;
        public FileInfo _fileInfo;
        public long? _toTalLengthUnzip = 0;
        int progressbarValue = 0;
        public static bool CheckForInternetConnection(int timeoutMs = 10000, string url = null)
        {
            try
            {
                CultureInfo cultureInfo = CultureInfo.InstalledUICulture;
                if (cultureInfo.Name.StartsWith("fa")) // Iran
                    url = "http://www.aparat.com";
                else if (cultureInfo.Name.StartsWith("zh"))  // China
                    url = "http://www.baidu.com";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }
        Task taskCopyStream;
        Stream streamTest;

        private void btnRetry_Click(object sender, EventArgs e)
        {
            btnRetry.Visible = false;//ẩn nút Retry
            threadExtect = new Thread(() =>
            {
                try
                {
                    streamTest.CopyTo(fileStream); //Try catch ở đây để khi lỗi do Inner- abort thread
                }
                catch (IOException) //Lỗi do DNS chẳng hạn- khi bật tắt ứng dụng 1.1.1.1 trong quá trình tải thì đều bị
                {
                    //Cho phép nút retry hiển thị để thử tải xuống tiếp
                    // throw new IOException(); //không ném lỗi ở đây được vì nó ở trong thread này
                    btnRetry.Invoke(new MethodInvoker(() => btnRetry.Visible = true));
                }
                catch { } //Lỗi khác thì không quan tâm
                tcs.SetResult(Sentinel);
            });
            threadExtect.Start();
        }

        FileStream fileStream;

        private async Task<bool> DownloadNewVersion(string path, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                Directory.CreateDirectory(path);// Tạo thư mục lưu dữ liệu
                Task taskRefreshFile;
                //Đặt timeout sau 5s k tải được do mất mạng thì xuất ra lỗi
                //Nếu không nó cứ đứng yên suốt
                //Nếu sau 10s từ lúc tắt mạng trước khi bật tải lên thì bắt được lỗi tại "catch (TaskCanceledException taskEx)" 
                using (HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(3) })
                    streamTest = await _httpClient.GetStreamAsync(_updateVersion.Url); 

                //Tạo đường link thư mục  temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}/download.zip
                _downloadZipPath = Path.Combine(path, Path.GetFileName(_updateVersion.Url));
                fileStream = File.Create(_downloadZipPath); //Tạo file download.zip

                tcs = new TaskCompletionSource<object>();
                _fileInfo = new FileInfo(_downloadZipPath);
                //File đã được tạo ở trên nên không lỗi được ở AutoRefresh => File.Create(_downloadZipPath))
                taskRefreshFile = AutoRefreshAsync(_fileInfo, _toTalLengthUnzip); //Bắt đầu đọc dung lượng hiện tại luôn ở đây
                                                                                  // await streamTest.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                                                                                  //taskCopyStream = streamTest.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                threadExtect = new Thread(() =>
                {
                    try
                    {
                        streamTest.CopyTo(fileStream); //Try catch ở đây để khi lỗi do Inner- abort thread
                        tcs.SetResult(Sentinel); //Nếu ở trên bị lỗi thì nhảy xuống chứ k qua thằng này
                    }

                    //Do không dùng using nên không xảy ra lỗi khi nhấn cancel bởi những lỗi như kết thúc using thì fileStream bị gần như huỷ...
                    catch (IOException ioex) //Lỗi do DNS chẳng hạn- khi bật tắt ứng dụng 1.1.1.1 trong quá trình tải thì đều bị
                    {
                        //1. Unable to read data from the transport connection: An established connection was aborted by the software in your host machine
                        //Hoặc : Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.
                        //lỗi 1 do bật tắt 1.1.1.1

                        if (ioex.Message == "Unable to read data from the transport connection: An established connection was aborted by the software in your host machine."
                        || ioex.Message== "Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.")
                        {
                            this.Invoke(new MethodInvoker(() => {
                                btnRetry.Visible = true;
                                label1.Text = "Error: network error";
                            }));
                            
                        }

                        //2. The read operation failed, see inner exception- Lỗi do huỷ thread(lệnh Abort) khi nhấn cancel

                        //Cho phép nút retry hiển thị để thử tải xuống tiếp
                        // throw new IOException(); //không ném lỗi ở đây được vì nó ở trong thread này
                      
                    }
                    catch (Exception ex)
                    {
                        //1. Thread be aboarted. do nhấn nút huỷ thì thread nó huỷ 
                    } //Lỗi khác thì không quan tâm
                   // tcs.SetResult(Sentinel); //Đưa thằng này lên trên
                });
                threadExtect.Start();

                await Task.WhenAny(tcs.Task); 

                await Task.WhenAny(taskRefreshFile);
                //if (_lengthTemp == _toTalLengthUnzip)
                //    return true;
                //else return false;
                if (cancellationTokenSource.IsCancellationRequested)
                    throw new TaskCanceledException();
                return true;
            }
            catch (TaskCanceledException taskEx) //Không bắt lỗi ở đây được nữa mà lỗi luôn Faulted
            {
                //1. Lỗi ở dòng này  streamTest = await _httpClient.GetStreamAsync(_updateVersion.Url); - A task was canceled.
                //Huỷ quá trình tải xuống
                return false;
            }
            catch (Exception ex)
            {
                var s = taskCopyStream.AsyncState;
                //Nếu xảy ra lỗi trong quá trình tải
                // MessageBox.Show(ex.Message);
                label1.Text = ex.Message;
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
                        if (_lengthTemp == value || isStopRefresh)
                            break;
                    }
                    catch { }
                }
            });
        }
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _cancellationTokenSourceCopy = new CancellationTokenSource();
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
            lbPercent.Text = "0 %";
            progressBar1.Value = 0;
            btnRetry.Visible = false;
            _currentIndexLength = 0; //Đặt lại số tệp tin trong file

            Guid guid = GetGuid(Assembly.GetExecutingAssembly());
            string path = Path.Combine(Path.GetTempPath(), "{" + guid.ToString().ToUpper() + "}"); //Tạo path: temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}

            //Kiểm tra Internet trước khi tải
            //if (!CheckForInternetConnection(1000, "http://www.google.com"))
            //{
            //    Cancelclosed = false; //Cho phép đóng form
            //    progressBar1.Visible = true;
            //    label1.Text = "Error: network error";
            //    lbPercent.Visible = true;
            //    return;
            //}
            //Tiến hành tải và đợi xong
            //Đợi download xong
            bool isDownloaded = false;
            try
            {
                isDownloaded = await DownloadNewVersion(path, _cancellationTokenSource);
            }
            catch (IOException ex)
            {
                MessageBox.Show("IO loi");
            }
            _currentIndexLength = 0; //Đặt lại số tệp tin trong file


            if (isDownloaded) //Nếu tải thành công
            {
                //Giải nén file
                _toTalLengthUnzip = 0;//Đặt lại dung lượng tổng trước khi extract
                await Task.Delay(500);// Delay 0.5s rồi tiến hành Extract
                label1.Text = "Uncompressing...";

                //Tiến hành Extract
                bool isExtracted = await ExtractAsync(_cancellationTokenSource); //Trong này đã kèm quá trình thanh tiến trình
                if (isExtracted) //Nếu giải nén thành công
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
                                      //progressBar1.Visible = false;
                                      // label1.Text = "Failed to download version.";
                                      //lbPercent.Vi//sible = false;
                this.Close();
            }

        }
        string pathFolder = string.Empty;
        Thread threadExtect;
        int khoa = 0;
        TaskCompletionSource<object> tcs;
        private static readonly object Sentinel = new object();
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
                    khoa = 0;
                    tcs = new TaskCompletionSource<object>();
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
                        //Task taskExtract = Task.Run(() => item.ExtractToFile(path, true), cancellationTokenSource.Token);
                        threadExtect = new Thread(() =>
                        {
                            item.ExtractToFile(path, true);
                            khoa = 1;
                            tcs.SetResult(Sentinel);
                        });

                        threadExtect.Start();

                        //await Task.Run(() =>
                        //{
                        //    while (khoa==0)
                        //    {
                        //        if (khoa == 1)
                        //            break;
                        //    }
                        //});

                        //Test2
                        await tcs.Task;


                        if (threadExtect.ThreadState == System.Threading.ThreadState.Stopped)
                        {

                        }
                        //await Task.WhenAny(taskExtract); //Đợi giải nén xong

                        //Đợi ghi xong thanh extract- Thằng này đang chạy ở trên rồi
                        await Task.WhenAny(taskRefreshFile);

                        //Khi nhấn huỷ
                        if (cancellationTokenSource.IsCancellationRequested)
                            throw new TaskCanceledException();

                        //if (taskExtract.Status != TaskStatus.RanToCompletion)
                        //    throw new Exception();//Khi đang extract mà bị lỗi...
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
                        //MessageBox.Show(ex.Message);
                        label1.Text = ex.Message;
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
            //if (!CheckForInternetConnection(1000, "http://www.google.com") && streamTest != null)
            if (streamTest != null)
                streamTest.Dispose(); //Không cần thằng này vì đã đặt tcs.SetResult(Sentinel); rồi
            if (fileStream != null)
                fileStream.Dispose();
            _cancellationTokenSource.Cancel();
            isStopRefresh = true; //Khi nhấn huỷ thì báo cho AutoRefreshAsync đóng lại
                                  //   this.Close(); // Không gọi nó ở đây mà đợi các task khác đóng lại hết đã rồi mới Close ở vị trí khác
                                  //Đợi các task đang thực hiện- Nói cách khác là huỷ các task khác thì ct tự động đóng
                                  //test
            if (threadExtect != null)
            {
                //khoa = 1;
                threadExtect.Abort(); //Phải huỷ Thread thì mới được - Vì Thread này chưa huỷ do - streamTest.CopyTo(fileStream);
                //chưa đóng lại khi nhấn cancel

                if (tcs != null && tcs.Task.Status != TaskStatus.RanToCompletion)
                    tcs.SetResult(Sentinel);
                //threadExtect = null;
            }
        }
        private void frmDownloadAndExtract_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Cancel();
            isStopRefresh = true;
            e.Cancel = Cancelclosed;
        }


        //_httpClient khi hết thì dispose luôn, streamTest thì là biến bên ngoài nên k dispose khi hoàn thành
        private async Task<bool> DownloadNewVersion1(string path, CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                Directory.CreateDirectory(path);// Tạo thư mục lưu dữ liệu
                Task taskRefreshFile;
                //Đặt timeout sau 5s k tải được do mất mạng thì xuất ra lỗi
                //Nếu không nó cứ đứng yên suốt
                using (HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(5) })
                using (streamTest = await _httpClient.GetStreamAsync(_updateVersion.Url))
                {
                    //Tạo đường link thư mục  temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}/download.zip
                    _downloadZipPath = Path.Combine(path, Path.GetFileName(_updateVersion.Url));
                    using (var fileStream = File.Create(_downloadZipPath)) //Tạo file download.zip
                    {
                        tcs = new TaskCompletionSource<object>();
                        _fileInfo = new FileInfo(_downloadZipPath);
                        //File đã được tạo ở trên nên không lỗi được ở AutoRefresh => File.Create(_downloadZipPath))
                        taskRefreshFile = AutoRefreshAsync(_fileInfo, _toTalLengthUnzip); //Bắt đầu đọc dung lượng hiện tại luôn ở đây
                                                                                          // await streamTest.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                                                                                          //taskCopyStream = streamTest.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                        threadExtect = new Thread(() =>
                        {
                            try
                            {
                                streamTest.CopyTo(fileStream); //Try catch ở đây để khi lỗi do Inner- abort thread
                                //Báo thông tin là đã tải xong - Khi dòng ở trên tải xong không có lỗi mới nhảy xuống được dòng này, nếu có lỗi ở dòng trên thì nhảy xuống lỗi luôn mà không qua dòng này
                                tcs.SetResult(Sentinel); 
                            }
                            catch (IOException ioex) //Lỗi do DNS chẳng hạn- khi bật tắt ứng dụng 1.1.1.1 trong quá trình tải thì đều bị
                            {
                                //Lỗi khi nhấn nút cancel - 
                                //Tại vì  await Task.WhenAny(tcs.Task); sẽ đợi xong rồi nên  streamTest.CopyTo(fileStream); lỗi do
                                //fileStream đã đóng lại bởi using
                                //1.Nếu dùng   if (streamTest != null)
                                // streamTest.Dispose(); trong nút Cancel thì xảy ra lỗi IOException ở đây
                                //2.The read operation failed, see inner exception.

                                //Cho phép nút retry hiển thị để thử tải xuống tiếp
                                btnRetry.Invoke(new MethodInvoker(() => btnRetry.Visible = true));
                            }
                            catch (Exception ex)

                            //Thread was being aborted. ++lỗi khi nhấn nút cancel và nó huỷ thread ở đây
                            // Cannot access a closed file. ++lỗi khi nhấn nút cancel.
                            { } //Lỗi khác thì không quan tâm
                            
                        });
                        threadExtect.Start();
                        //  await tcs.Task;

                        //    await Task.WhenAny(taskCopyStream); //Vì dùng WhenAny nên cancellationTokenSource.Token không xảy ra
                        //await stream.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                        //await Task.Run(() => {
                        //    stream.CopyTo(fileStream);
                        //},cancellationTokenSource.Token);
                        //await Task.WhenAll(tcs.Task, taskRefreshFile);

                        //Chỉ nhấn F10 để bắt lỗi- nhấn F5 nó tuột đi luôn không quan sát được
                        //24/09/2024- Khi đang tải mà tắt 1.1.1.1 hoặc đang tải mà bật 1.1.1.1 thì bị thoát ở đây và đợi mãi ở await phía dưới
                        //Cái Easybuilder cũng bị lỗi tương tự khi cập nhật phần mềm
                        //Nếu đang tải với 1.1.1.1 mà ngắt kết nối hoàn toàn thì vẫn xác định tuột như Easybuilder
                        await Task.WhenAny(tcs.Task); // Code chạy xuống đây trước rồi mới vào Thread- nên là khi nhấn huỷ thì cái này xong
                        //tức là using (var fileStream)... đóng lại cả đã đoạn code này => fileStream bị huỷ =>  streamTest.CopyTo(fileStream); ở trong thread bị lỗi ở fileStream
                        
                    }

                }

                //if (!CheckForInternetConnection(1000, "http://www.google.com"))
                //{
                //    MessageBox.Show("Không có internet");
                //}
                //else
                //    MessageBox.Show("Có internet");



                //Đợi đến khi thanh hiển thị đủ 100%
                await Task.WhenAny(taskRefreshFile);
                //if (_lengthTemp == _toTalLengthUnzip)
                //    return true;
                //else return false;
                if (cancellationTokenSource.IsCancellationRequested)
                    throw new TaskCanceledException();
                return true;
                //if (taskCopyStream.Status == TaskStatus.Faulted) //Do  Dispose streamTest nên taskCopyStream bị lỗi Faulted 
                //{ return false; }
                //else { return true; }
                // Vì  await Task.WhenAny(taskCopyStream); không ném thẳng ra lỗi mà nó sẽ để trạng thái không hoàn thành tại đây luôn 
                //if (taskCopyStream.Status != TaskStatus.RanToCompletion) //Do  Dispose streamTest nên taskCopyStream bị lỗi Faulted 
                //{ return false; }
                //else { return true; }
            }
            catch (TaskCanceledException taskEx) //Không bắt lỗi ở đây được nữa mà lỗi luôn Faulted
            {
                //Huỷ quá trình tải xuống
                return false;
            }
            catch (Exception ex)
            {
                var s = taskCopyStream.AsyncState;
                //Nếu xảy ra lỗi trong quá trình tải
                // MessageBox.Show(ex.Message);
                label1.Text = ex.Message;
                return false;
            }
        }
    }
}
