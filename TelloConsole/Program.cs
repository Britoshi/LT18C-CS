using Brito;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelloLib;

namespace TelloConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //subscribe to Tello connection events
            Tello.onConnection += (Tello.ConnectionState newState) =>
            {
                if (newState != Tello.ConnectionState.Connected)
                {

                }
                if (newState == Tello.ConnectionState.Connected)
                {
                    Tello.queryAttAngle();
                    Tello.setMaxHeight(50);

                    clearConsole();
                }
                printAt(0,0,"Tello " + newState.ToString());
            };

            //Log file setup.
            var logPath = "logs/";
            System.IO.Directory.CreateDirectory(Path.Combine("../", logPath));
            var logStartTime = DateTime.Now;
            var logFilePath = Path.Combine("../", logPath + logStartTime.ToString("yyyy-dd-M--HH-mm-ss") + ".csv");

            //write header for cols in log.
            File.WriteAllText(logFilePath, "time,"+Tello.state.getLogHeader());

                //subscribe to Tello update events.
            Tello.onUpdate += (cmdId) =>
            {
                if (cmdId == 86)//ac update
                {
                    //write update to log.
                    var elapsed = DateTime.Now - logStartTime;
                    File.AppendAllText(logFilePath, elapsed.ToString(@"mm\:ss\:ff\,") + Tello.state.getLogLine());

                    //display state in console.
                    var outStr = Tello.state.ToString();//ToString() = Formated state
                    printAt(0, 2, outStr);

                    //Now do the other stuff.
                    Mission.Update();
                }
            };
             

            //subscribe to Joystick update events. Called ~10x second.
            PCKeyboard.onUpdate += (KeyboardState keyState) =>
            {

                Single rX = 0, rY = 0;
                Single lX = 0, lY = 0;

                Single boost = 0;

                if (keyState.IsPressed(Key.A))
                    rX--;
                if (keyState.IsPressed(Key.D))
                    rX++; 
                if (keyState.IsPressed(Key.W))
                    rY++;
                if (keyState.IsPressed(Key.S))
                    rY--;

                if (keyState.IsPressed(Key.Q))
                    lX--;
                if (keyState.IsPressed(Key.E))
                    lX++;
                if (keyState.IsPressed(Key.Space))
                    lY++; 
                if (keyState.IsPressed(Key.LeftControl))
                    lY--; 

                if (keyState.IsPressed(Key.LeftShift))
                    boost++; 


                    //var boost = joyState.Z
                float[] axes = new float[] { 
                    lX + Mission.Instance.AxisModification[0],
                    lY + Mission.Instance.AxisModification[1],
                    rX + Mission.Instance.AxisModification[2],
                    rY + Mission.Instance.AxisModification[3], 
                    boost };
                var outStr = string.Format("JOY {0: 0.00;-0.00} {1: 0.00;-0.00} {2: 0.00;-0.00} {3: 0.00;-0.00} {4: 0.00;-0.00}", axes[0], axes[1], axes[2], axes[3], axes[4]);
                MyPrintAt(0, 22, outStr);
                MyPrintAt(0, 15, Mission.Instance.AxisModification.ToString());
                Tello.controllerState.setAxis(lX, lY, rX, rY);
                Tello.sendControllerUpdate();
            };
            PCKeyboard.init();

            //Connection to send raw video data to local udp port.
            //To play: ffplay -probesize 32 -sync ext udp://127.0.0.1:7038
            //To play with minimum latency:ffmpeg -i udp://127.0.0.1:7038 -f sdl "Tello"
            var videoClient = UdpUser.ConnectTo("127.0.0.1", 7038);

            //subscribe to Tello video data
            Tello.onVideoData += (byte[] data) =>
            {
                try
                {
                    videoClient.Send(data.Skip(2).ToArray());
                    
                    

                    //Skip 2 byte header and send to ffplay. 
                    //Console.WriteLine("Video size:" + data.Length);
                }
                catch (Exception ex)
                {

                }
            };

            Tello.startConnecting();//Start trying to connect.

            clearConsole();

            var str = "";
            while(str!="exit")
            {
                str = Console.ReadLine().ToLower();
                var condition = Tello.connected && !Tello.state.flying;

                if (str == "takeoff" && condition)
                    Tello.takeOff();
                if (str == "land" && condition)
                    Tello.land();
                if (str == "cls")
                {
                    Tello.setMaxHeight(9);
                    Tello.queryMaxHeight();
                    clearConsole();
                }
                if(str.Contains("floor"))
                {
                    MyPrintAt(0, 1,"FLooring");
                    Mission.SetLockHeight(100); 
                }
                if (str.Contains("ceiling"))
                {
                    MyPrintAt(0, 1, "Celing");
                    Mission.SetLockHeight(200);
                }
            }
        }
        //Print at x,y in console. 
        static void printAt(int x, int y, string str)
        {
            var saveLeft = Console.CursorLeft;
            var saveTop = Console.CursorTop;
            //Console.SetCursorPosition(x, y);
            //Console.WriteLine(str + "     ");//Hack. extra space is to clear any previous chars.
            //Console.SetCursorPosition(saveLeft, saveTop);

        }

        static void MyPrintAt(int x, int y, string str)
        {
            var saveLeft = Console.CursorLeft;
            var saveTop = Console.CursorTop;
            Console.SetCursorPosition(x, y);
            Console.WriteLine(str + "     ");//Hack. extra space is to clear any previous chars.
            Console.SetCursorPosition(saveLeft, saveTop);
        }

        static void clearConsole()
        {
            Console.Clear();
            Console.SetCursorPosition(0, 23);
            Console.WriteLine("Commands:takeoff,land,exit,cls");
        }
    }
}
