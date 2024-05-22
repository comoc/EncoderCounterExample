using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.EditorTools;
using UnityEngine;

public class EncoderCounter : MonoBehaviour
{
    [Tooltip("Specify the serial port name")]
    [SerializeField] private string serialPortName = "COM1";
    
    public bool IsAvailable { get; private set; } = false;
    public long Count { get; private set; } // エンコーダのカウント値
    public long NumberOfRevolution { get; private set; } // エンコーダの回転数
    private long lastCount = 0;
    
    private static readonly int BaudRate = 921600;
    private static readonly int PulsePerRevolution = 400; // エンコーダ1回転あたりのパルス数
    private Task task;
    private CancellationTokenSource cancel;

    // Start is called before the first frame update
    void Awake()
    {
        IsAvailable = false;
        Count = 0;
        NumberOfRevolution = 0;
        lastCount = 0;

        int n = 0;
        foreach (var port in SerialPort.GetPortNames())
        {
            Debug.Log($"Serial port [{n}]: {port}");
            n++;
        }

        cancel = new CancellationTokenSource();
        task = Task.Run(() =>
        {
            SerialPort serialPort = null;
            try
            {
                serialPort = new SerialPort(serialPortName, BaudRate, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 100,
                    WriteTimeout = 100,
                    NewLine = "\n" // In case of Unity, must specify the newline character to read the data correctly
                };
                serialPort.Open();
                IsAvailable = true;
                while (!cancel.Token.IsCancellationRequested)
                {
                    string line = serialPort.ReadLine();
                    string[] values = line.Split(':');
                    if (values.Length == 0)
                    {
                        Debug.LogError("Invalid data: " + line);
                        continue;
                    }

                    long currentCount = int.Parse(values[0]);

                    // int値のオーバーフローとアンダーフローを考慮してcountを計算する。合ってるか自信が無いので、もし間違っていたら教えてください。
                    // オーバーフローした場合
                    if (lastCount - currentCount > int.MaxValue)
                    {
                        currentCount = (long)int.MaxValue + currentCount - (long)int.MinValue + 1;
                    }
                    // アンダーフローした場合
                    else if (lastCount - currentCount < int.MinValue)
                    {
                        currentCount = (long)int.MinValue + currentCount - (long)int.MaxValue - 1;
                    }
                    Count += currentCount - lastCount;
                    
                    NumberOfRevolution = Count / PulsePerRevolution;
                    lastCount = currentCount;

                    Task.Yield();
                }
            }
            catch (TimeoutException)
            {
                Debug.LogWarning("Timeout");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            IsAvailable = false;

            if (serialPort != null && serialPort.IsOpen)
                serialPort.Close();

        }, cancel.Token);
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: Implement your logic here
        // Debug.Log($"Count: {count}, Revolution: {numberOfRevolution}");
    }

    private void OnDestroy()
    {
        IsAvailable = false;
        cancel.Cancel();
        task.Wait();
    }
}
