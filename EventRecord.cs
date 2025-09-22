// -----------------------------------------------------------------------------
// Event Record Function
//
// 이 코드는 제가 직접 개발한 기능을 바탕으로, 보안상 실제 업무 코드를 그대로 공개하지 않고
// 포트폴리오 용도로 일부 수정·단순화하여 작성한 버전입니다.
// 목적은 "개인 포트폴리오"이며, 외부 연구/학습/데모를 위한 예제 코드 제공이 아닙니다.
//
// 기능 요약:
// - UDP 이벤트(Trigger/StartStop)를 받아 가상의 DB에 Insert/Update 수행
// - 데모 전용 DB 계정 및 가상 포트/파일 형식 코드 사용
// - 실제 업무 로직과는 다른 단순화된 구조
//
// 주의사항:
// - 본 코드는 공개용 데모로, 실제 장비/업체의 포맷을 반영하지 않습니다.
// - 변수명, 테이블명, 데이터 형식은 모두 공개용으로 임의 설정된 것입니다.
// - 구조와 변수명은 공개용으로 임의 설정된 것입니다.
// -----------------------------------------------------------------------------

public delegate void DemoUdpEventHandler(string item);

public class EventRecord
{
    private SqlConnection connMain;
    private SqlConnection connSub;
    private bool isDbConnected = false;

    // 로그 출력
    private void LogMessage(string msg)
    {
        Console.WriteLine(msg);
    }

    // DB 연결 초기화
    public void InitDbConnections(string mainServer, string subServer)
    {
        string connString_Main = $"User ID=demoUser; Password=demoPass123; Data Source={mainServer}; Initial Catalog=DemoDatabase;";
        connMain = new SqlConnection(connString_Main);

        string connString_Sub = $"User ID=demoUser; Password=demoPass123; Data Source={subServer}; Initial Catalog=DemoDatabase;";
        connSub = new SqlConnection(connString_Sub);

        try
        {
            connMain.Open();
            connSub.Open();
            isDbConnected = true;
        }
        catch (Exception)
        {
            LogMessage("Database connection failed (demo).");
            isDbConnected = false;
        }
    }

    // 프로그램 전체 종료시 DB 연결 종료
    public void CloseConnections()
    {
        if (connMain != null && connMain.State == ConnectionState.Open)
            connMain.Close();
        connMain?.Dispose();//null 연산자

        if (connSub != null && connSub.State == ConnectionState.Open)
            connSub.Close();
        connSub?.Dispose();//null 연산자
    }

    // UDP 트리거 이벤트
    public void OnUdpTrigger(string str)
    {
        if (this.NeedsInvoke()) // UI 스레드 체크 (가상)
        {
            DemoUdpEventHandler d = new DemoUdpEventHandler(OnUdpTrigger);
            this.Invoke(d, new object[] { str });
        }
        else
        {
            string saveFolder = txtDemoTriggerPath.Text;
            string saveType = "Trigger_";
            lblUdpStatus.Text = "DEMO_TRIGGER";

            if (string.IsNullOrEmpty(currentFileName))
            {
                LogMessage("Trigger event received.");
                currentFileName = str;
            }
        }
    }

    // UDP 시작/종료 이벤트
    public void OnUdpStartStop(string str)
    {
        if (this.NeedsInvoke())
        {
            DemoUdpEventHandler d = new DemoUdpEventHandler(OnUdpStartStop);
            this.Invoke(d, new object[] { str });
        }
        else
        {
            string saveFolder = txtDemoStartStopPath.Text;
            string saveType = "StartStop_";
            lblUdpStatus.Text = "DEMO_STARTSTOP";

            if (string.IsNullOrEmpty(currentFileName))
            {
                LogMessage("Start/Stop event received.");
                currentFileName = str;
            }
        }
    }

    // 트리거 이벤트 DB 입력 (Main DB)
    public void InsertTriggerEvent(string dateTime, string fileName)
    {
        using (SqlCommand cmd = connMain.CreateCommand())
        {
            cmd.CommandText = string.Format(
                "INSERT INTO demo_trigger_data (data_type, data_time, data_file, data_ch) " +
                "VALUES(0x3000, '{0}', '{1}', 0xFFFF)", dateTime, fileName);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                LogMessage("Database Insert failed.");
            }
        }
    }

    // 시작/종료 이벤트 DB 갱신 (Sub DB)
    public void UpdateStartStopEvent(string fileName, int demoId)
    {
        using (SqlCommand cmd = connSub.CreateCommand())
        {
            cmd.CommandText = string.Format(
                "UPDATE demo_startstop_data SET data_file = '{0}' WHERE data_id = '{1}'",
                fileName, demoId);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                LogMessage("Database Update failed.");
            }
        }
    }

    // 도움말 기능 버튼 클릭 시 PDF 매뉴얼 뷰어 열기
    private void OpenManual(object sender, EventArgs e) 
    {
        HelpPdfForm manualViewer = new HelpPdfForm(); // PdfiumViewer 기반 폼
        manualViewer.Show();
    }

}




