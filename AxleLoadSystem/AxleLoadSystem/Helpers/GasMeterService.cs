using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace AxleLoadSystem.Helpers;

public interface IGasMeterService
{
    Task<bool> ConnectAsync(string host, int port);
    Task DisconnectAsync();
    Task<string> ReadMeterDataAsync(string meterNo);
    Task<string> DisconnectMeterAsync(string meterNo, DateTime dateTime, double leftVal, int valveType);
    Task<string> DailyReadDataAsync(string meterNo, string startMMDD, string endMMDD, int frameStatus);
    Task<string> MonthlyReadDataAsync(string meterNo, string startYYMM, string endYYMM, int frameStatus);
    Task<string> CheckTimeAsync(string meterNo, DateTime dateTime);
    Task<string> ValveControlAsync(string meterNo, int valveControl);
    Task<string> ParameterSettingAsync(string meterNo, object settings);
    Task<string> CommunicationParameterSettingAsync(string meterNo, object settings);
    Task<string> ReadEventRecordAsync(string meterNo, int frameStatus);
    Task<string> ReportModeAsync(string meterNo, int reportMode, int days, int nextReportMode);
    Task<string> SetPlanManagementAsync(string meterNo, int planOn, string valveCloseDate, double creditVal);
    Task<string> DisplayRemainingAmountAsync(string meterNo, int displayOn, string settlementDate, double residualVal, int accountStatus);
}

public class GasMeterService : IGasMeterService
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly string _dllPath;
    private dynamic? _sdkInstance;

    public GasMeterService()
    {
        // Keep DLL in a non-public "lib" folder (same level as wwwroot)
        var _dllPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\lib", "NBGasMeter2.dll");


        try
        {
            if (File.Exists(_dllPath))
            {
                var asm = Assembly.LoadFrom(_dllPath);
                foreach (var t in asm.GetTypes())
                {
                    if (t.GetMethod("GetSendData") != null)
                    {
                        _sdkInstance = Activator.CreateInstance(t);
                        break;
                    }
                }
            }
        }
        catch
        {
            _sdkInstance = null; // Fallback mode
        }
    }

    #region Connection Management

    public async Task<bool> ConnectAsync(string host, int port)
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            if (_stream != null)
                await _stream.DisposeAsync();

            _client?.Close();
            _client = null;
            _stream = null;
        }
        catch { }
    }

    private async Task<string> SendCommandAsync(string meterNo, int cmdType, object inData)
    {
        if (_stream == null)
            throw new InvalidOperationException("Not connected to device.");

        byte[] sendBytes;

        // Convert input JSON to byte[] using SDK (if loaded)
        if (_sdkInstance != null)
        {
            sendBytes = (byte[])_sdkInstance.GetSendData(meterNo, cmdType, JsonConvert.SerializeObject(inData));
        }
        else
        {
            var payload = new { MeterNO = meterNo, cmdType, request = inData };
            sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
        }

        await _stream.WriteAsync(sendBytes, 0, sendBytes.Length);

        // Read response
        var buffer = new byte[8192];
        var len = await _stream.ReadAsync(buffer, 0, buffer.Length);
        var respBytes = new byte[len];
        Array.Copy(buffer, respBytes, len);

        string parsed;
        if (_sdkInstance != null)
            parsed = _sdkInstance.GetReceiveData(respBytes);
        else
            parsed = Encoding.UTF8.GetString(respBytes);

        return parsed;
    }

    #endregion

    #region SDK Command Wrappers

    public Task<string> ReadMeterDataAsync(string meterNo)
        => SendCommandAsync(meterNo, 0, new { });

    public Task<string> DisconnectMeterAsync(string meterNo, DateTime dateTime, double leftVal, int valveType)
        => SendCommandAsync(meterNo, 1, new
        {
            dateTime = dateTime.ToString("yyyyMMddHHmmss"),
            checkTime = 0,
            leftVal,
            valveType
        });

    public Task<string> DailyReadDataAsync(string meterNo, string startMMDD, string endMMDD, int frameStatus)
        => SendCommandAsync(meterNo, 2, new { start = startMMDD, end = endMMDD, frameStatus });

    public Task<string> MonthlyReadDataAsync(string meterNo, string startYYMM, string endYYMM, int frameStatus)
        => SendCommandAsync(meterNo, 3, new { start = startYYMM, end = endYYMM, frameStatus });

    public Task<string> CheckTimeAsync(string meterNo, DateTime dateTime)
        => SendCommandAsync(meterNo, 4, new { dateTime = dateTime.ToString("yyyyMMddHHmmss") });

    public Task<string> ValveControlAsync(string meterNo, int valveControl)
        => SendCommandAsync(meterNo, 5, new { valveControl });

    public Task<string> ParameterSettingAsync(string meterNo, object settings)
        => SendCommandAsync(meterNo, 6, settings);

    public Task<string> CommunicationParameterSettingAsync(string meterNo, object settings)
        => SendCommandAsync(meterNo, 7, settings);

    public Task<string> ReadEventRecordAsync(string meterNo, int frameStatus)
        => SendCommandAsync(meterNo, 8, new { frameStatus });

    public Task<string> ReportModeAsync(string meterNo, int reportMode, int days, int nextReportMode)
        => SendCommandAsync(meterNo, 9, new { reportMode, days, nextReportMode });

    public Task<string> SetPlanManagementAsync(string meterNo, int planOn, string valveCloseDate, double creditVal)
        => SendCommandAsync(meterNo, 12, new
        {
            planOn,
            valveCloseDate,
            creditVal
        });

    public Task<string> DisplayRemainingAmountAsync(string meterNo, int displayOn, string settlementDate, double residualVal, int accountStatus)
        => SendCommandAsync(meterNo, 13, new
        {
            displayOn,
            settlementDate,
            residualVal,
            accountStatus
        });

    #endregion
}

