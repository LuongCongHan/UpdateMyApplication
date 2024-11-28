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
using System.Drawing;

namespace UpdateMyApp
{
    public partial class frmDownloadAndExtract : Form
    {
        public frmDownloadAndExtract()
        {
            InitializeComponent();
            lbPercent.MaximumSize = new Size(this.Size.Width - lbPercent.Location.X - 30, 0);
            label1.MaximumSize = new Size(this.Size.Width - lbPercent.Location.X - 30, 0);
            this.MaximizeBox = false;//Ngăn không cho phóng to form khi nhấn đúp vào thanh điêu đề ( title bar)
            methodInvoker = new MethodInvoker(() =>
            {
                progressBar1.Value = progressbarValue;
                try
                {
                    //Lỗi ở đây chưa giải quyết được
                    //Lỗi tại AccessibilityObject => CreateParams => có thể là do đóng chương trình nên lbPercent không thể truy cập được nữa
                    //'lbPercent.CreateParams' threw an exception of type 'System.InvalidOperationException'	System.Windows.Forms.CreateParams {System.InvalidOperationException}
                    //
                    lbPercent.Text = string.Format("{0} %", (int)((float)progressbarValue / int.MaxValue * 100)); //Hiển thị phần trăm nguyên
                }
                catch (Exception loiex)
                {
                    string m = loiex.Message;
                }
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
            catch(Exception ex)
            {
                return false;
            }
        }
        Stream streamTest;
      
        FileStream fileStream;
        bool isClickCancelOrClose = false;
        private async Task<bool> DownloadNewVersion(string path)
        {
            try
            {
                Directory.CreateDirectory(path);// Tạo thư mục lưu dữ liệu

                //Đặt timeout sau 5s k tải được do mất mạng thì xuất ra lỗi
                //Nếu không nó cứ đứng yên suốt
                //Nếu sau 10s từ lúc tắt mạng trước khi bật tải lên thì bắt được lỗi tại "catch (TaskCanceledException taskEx)" 
                using (HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(2) })
                    streamTest = await _httpClient.GetStreamAsync(_updateVersion.Url);

                //await Task.Delay(10000);

                //Tạo đường link thư mục  temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}/download.zip
                _downloadZipPath = Path.Combine(path, Path.GetFileName(_updateVersion.Url));
                fileStream = File.Create(_downloadZipPath); //Tạo file download.zip

                tcs = new TaskCompletionSource<object>();
                _fileInfo = new FileInfo(_downloadZipPath);
                //File đã được tạo ở trên nên không lỗi được ở AutoRefresh => File.Create(_downloadZipPath))
                Task taskRefreshFile = AutoRefreshAsync(_fileInfo, _toTalLengthUnzip); //Bắt đầu đọc dung lượng hiện tại luôn ở đây
                                                                                       // await streamTest.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                                                                                       //taskCopyStream = streamTest.CopyToAsync(fileStream, 4096, cancellationTokenSource.Token);
                threadExtect = new Thread(() =>
                {
                    try
                    {
                        streamTest.CopyTo(fileStream); //Try catch ở đây để khi lỗi do Inner- abort thread
                        //tcs.SetResult(Sentinel); //Nếu ở trên bị lỗi thì nhảy xuống chứ k qua thằng này
                    }

                    //Do không dùng using nên không xảy ra lỗi khi nhấn cancel bởi những lỗi như kết thúc using thì fileStream bị gần như huỷ...
                    catch (Exception ex) //Lỗi do DNS chẳng hạn- khi bật tắt ứng dụng 1.1.1.1 trong quá trình tải thì đều bị
                    {
                        timerCheckInternet.Stop();
                        //1. Unable to read data from the transport connection: An established connection was aborted by the software in your host machine
                        //Hoặc : Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host.
                        //lỗi 1 do bật tắt 1.1.1.1

                        //2. The read operation failed, see inner exception- Lỗi do huỷ thread(lệnh Abort) khi nhấn cancel

                        //Cho phép nút retry hiển thị để thử tải xuống tiếp
                        // throw new IOException(); //không ném lỗi ở đây được vì nó ở trong thread này
                        // Do ex 1. Thread be aboarted. do nhấn nút huỷ thì thread nó huỷ => không cần hiển thị lỗi làm gì
                    }
                    finally
                    {
                        //Nếu nhấn cancel thì dòng này : tcs.SetResult(Sentinel) sẽ lỗi do nó đã hoàn thành
                        //Khi nhấn cancel thì có thể 2 dòng await xong trước cái này luôn rồi

                        // tcs.SetResult(Sentinel); //=> để đóng hoàn thành việc đợi tại đây  await Task.WhenAny(tcs.Task);

                        if (streamTest != null)
                            streamTest.Dispose(); //Không cần thằng này vì đã đặt tcs.SetResult(Sentinel); rồi
                        if (fileStream != null)
                            fileStream.Dispose();

                        if (tcs != null && tcs.Task.Status != TaskStatus.RanToCompletion)
                            tcs.SetResult(Sentinel);
                        isStopRefresh = true; //Đóng cái await Task.WhenAny(taskRefreshFile); lại luôn
                    }

                });
                threadExtect.Start();

                timerCheckInternet.Start();
                await Task.WhenAny(tcs.Task);

                await Task.WhenAny(taskRefreshFile);
                timerCheckInternet.Stop();
                if (_lengthTemp == _toTalLengthUnzip)
                    return true; //2 thằng bắt lỗi ở trên nhưng ở đây vẫn trả về TRUE-------------------phải xem lại
                else
                    return false;
            }
            catch
            {
                timerCheckInternet.Stop();
                return false;
            }
        }
        long _lengthTemp = 0;
        long _checkLengthTemp = -1;
        MethodInvoker methodInvoker;
        long _currentIndexLength = 0;
        bool isStopRefresh = false;
        private async Task AutoRefreshAsync(FileInfo file, long? value)
        {

            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        file.Refresh();//Làm mới file
                        _lengthTemp = _fileInfo.Length + _currentIndexLength;
                        progressbarValue = (int)(_lengthTemp * int.MaxValue / _toTalLengthUnzip);

                        // Object name: 'frmDownloadAndExtract'.'
                        if (progressbarValue < 0)
                        {
                            break;
                        }
                        this.Invoke(methodInvoker); //Xuất thông báo ra Progressbar - Lỗi ở đây khi nhấn nút tắt cửa sổ
                                                    //System.ObjectDisposedException: 'Cannot access a disposed object.

                        if (_lengthTemp == value || isStopRefresh)
                            break;
                    }
                    catch (Exception ex)
                    {
                        string t = ex.Message;
                        break; //Nếu lỗi do nhấn cancel- hay đóng form download thì lỗi  
                        //System.ObjectDisposedException: 'Cannot access a disposed object. sẽ xuất hiện thì ta thoát ra
                        //chứ không là nó mắc kẹt không thoát được mặc dù đã bật isStopRefresh=true
                    }
                }
            });
        }
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _cancellationTokenSourceCopy = new CancellationTokenSource();
        public bool isStartNewUpdate = false;
        private async void frmDownloadAndExtract_Load(object sender, EventArgs e)
        {
            lbPercent.Visible = true;
            progressBar1.Visible = true;
            _checkLengthTemp = -1;
            isClickCancelOrClose = false;
            if (_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();//Phải tạo mới nếu như lần trước đó huỷ thì khi mở lại thì các task không hoàn thành
            isStopRefresh = false; //Đặt lại giá trị cho hàm Autofresh
            isStartNewUpdate = false; //Thông báo là đang cập nhật phiên bản

            label1.Text = "Downloading...";
            lbPercent.Text = "0 %";
            progressBar1.Value = 0;
            _currentIndexLength = 0; //Đặt lại số tệp tin trong file

            Guid guid = GetGuid(Assembly.GetExecutingAssembly());
            string path = Path.Combine(Path.GetTempPath(), "{" + guid.ToString().ToUpper() + "}"); //Tạo path: temp/{4542C1F4-61C7-4C0D-A6EC-E805411B8184}

            bool isDownloaded = await DownloadNewVersion(path);

            _currentIndexLength = 0; //Đặt lại số tệp tin trong file
            if (isDownloaded) //Nếu tải thành công
            {
                //Giải nén file
                _toTalLengthUnzip = 0;//Đặt lại dung lượng tổng trước khi extract
                await Task.Delay(500);// Delay 0.5s rồi tiến hành Extract
                label1.Text = "Uncompressing...";

                ///////// test 
                // if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                //_cancellationTokenSource = new CancellationTokenSource();
                //_cancellationTokenSource.Cancel();

                //Tiến hành Extract
                bool isExtracted = await ExtractAsync(); //Trong này đã kèm quá trình thanh tiến trình
                if (isExtracted) //Nếu giải nén thành công
                {
                    await Task.Delay(100); //Đợi tý rồi kích hoạt
                                           //TopDirectoryOnly => tìm trong tệp ngoài không tìm tệp con
                                           //var exeFiles = Directory.EnumerateFiles(pathFolder, "*.exe", SearchOption.TopDirectoryOnly).ToArray();
                                           //var exeFiles = Directory.EnumerateFiles(pathFolder, _updateVersion.fileExtension, SearchOption.TopDirectoryOnly).ToArray();

                    var exeFiles = Directory.EnumerateFiles(pathFolder, "ece", SearchOption.TopDirectoryOnly).ToArray();

                    if (exeFiles != null && exeFiles.Length > 0 && 
                        Path.GetFileNameWithoutExtension(exeFiles[0])==_updateVersion.fileName  && !isClickCancelOrClose)
                    {

                        Process.Start(exeFiles[0]);
                        isStartNewUpdate = true; //Bắt đầu chạy version mới
                        this.Close();
                    }
                    else
                    {
                        //Không tìm thấy tệp tin để cài hoặc danh sách rỗng
                        //MessageBox.Show("The new version file was not found.", "Install");
                        label1.Text = "An error occurred while preparing for installation. Please try again later.";
                        //lbPercent.Location = new Point((this.ClientSize.Width - label1.Width) / 2, label1.Height);
                        lbPercent.Visible = false;
                        progressBar1.Visible = false;
                    }
                }
                else //Nếu giải nén thất bại
                {

                    label1.Text = "An error occurred while extracting the file. Please try again later.";
                    //lbPercent.Location = new Point((this.ClientSize.Width - label1.Width) / 2, label1.Height);
                    lbPercent.Visible = false;
                    progressBar1.Visible = false;
                    //this.Close();
                }
            }
            //////////////////// Xem lại chỗ này isClickCancel không có tác dụng gì cả
            //else if (isClickCancel) //Nếu tải thất bại- không cần thiết
            //{
            //                          //progressBar1.Visible = false;
            //                          // label1.Text = "Failed to download version.";
            //                          //lbPercent.Vi//sible = false;
            //   //Gọi close ở đây nữa sẽ bị lỗi vì close ở sự kiện click huỷ rồi
            //   // this.Close();
            //}
            else if(!isClickCancelOrClose) //Nếu nhấn đóng form đang tải thì nhảy luôn vào đây- phải giải quyết ở đây
            {
                //label1.Text = "The installation failed.";
                label1.Text = "An error occurred in retrieving update information; are you connected to the internet? Please try again later.";
                //lbPercent.Location = new Point((this.ClientSize.Width - label1.Width) / 2, label1.Height);
                lbPercent.Visible = false;
                progressBar1.Visible = false;
            }
        }
        string pathFolder = string.Empty;
        Thread threadExtect;
        int khoa = 0;
        TaskCompletionSource<object> tcs;
        private static readonly object Sentinel = new object();
        private async Task<bool> ExtractAsync()
        {
            //Mở file zip để đọc
            try
            {
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
                       // throw new Exception("loi ne may");
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

                            await tcs.Task;

                            //Đợi ghi xong thanh extract- Thằng này đang chạy ở trên rồi
                            await Task.WhenAny(taskRefreshFile); //Đợi giải nén xong


                            //if (taskExtract.Status != TaskStatus.RanToCompletion)
                            //    throw new Exception();//Khi đang extract mà bị lỗi...
                            _currentIndexLength += item.Length;

                            //Khi nhấn huỷ
                            if (isClickCancelOrClose) return false;

                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.Message);
                            label1.Text = ex.Message + " " + ex.HResult;
                            return false;
                        }
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Guid GetGuid(Assembly assembly)
        {
            var guidAttribute = (GuidAttribute)assembly?.GetCustomAttributes(typeof(GuidAttribute), false).SingleOrDefault();
            if (Guid.TryParse(guidAttribute?.Value, out Guid guid)) { return guid; }
            return Guid.Empty;
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {//-----Chưa giải quyết khi bị lỗi do bật tắt 1.1.1.1 thì nhấn cancel làm sao để đóng chương trình lại- khi nút retry được hiển thị
            //tức là hàm download đã hoàn thành và thoát ra rồi
           // isClickCancel = true;
            this.Close();
        }
        private void frmDownloadAndExtract_FormClosing(object sender, FormClosingEventArgs e)
        { //Chưa giải quyết được khi nhấn cancel hoặc nút x thì hàm AutoRefreshAsync vẫn còn hoạt động 
            isClickCancelOrClose = true;
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Cancel();
            //if (!CheckForInternetConnection(1000, "http://www.google.com") && streamTest != null)
            if (streamTest != null)
                streamTest.Dispose(); //Không cần thằng này vì đã đặt tcs.SetResult(Sentinel); rồi
            if (fileStream != null)
                fileStream.Dispose();

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
                    try { tcs.SetResult(Sentinel); } catch { }
                //threadExtect = null;
            }

        }
        private void timerCheckInternet_Tick(object sender, EventArgs e)
        {
            if (_checkLengthTemp != _lengthTemp) //ok k lỗi- dữ liệu cập nhật liên tục
            {
                _checkLengthTemp = _lengthTemp; // cập nhật lại dữ liệu
            }
            else if (!CheckForInternetConnection(500, "http://www.google.com")) //Nếu dữ liệu cập nhật không đổi thì check internet thử luôn
            {
                timerCheckInternet.Stop();
                if (streamTest != null)
                    streamTest.Dispose(); //đóng thằng này làm dừng chương trình
            }

        }

    }
}
