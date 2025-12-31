using Dln.Gpio;
using FZ4P.DriverIc.Interfaces;
using FZ4P.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FZ4P.DriverIc.Control
{
    public enum LoadState
    {
        Load = 0,
        Unloaded = 1,
    }
    public enum CoverState
    {
        Up = 0,
        Down = 1,
    }

    public class DLNIOControl : IDLNIOControl
    {
        private Module[] _gpio;
        object I2cLock = new object();

        public DLNIOControl(Module[] gpio)
        {
            _gpio = gpio;
        }

        /// <summary>
        /// Load
        /// SetError -> Exception Error 처리 상위 모듈에서 메세지 처리하세요.
        /// </summary>
        public void LoadSocket(LoadState state)
        {
            try
            {
                if (_gpio == null) throw new InvalidOperationException("GPIO module is not initialized.");

                switch (state)
                {
                    case LoadState.Load:
                        lock (I2cLock)
                        {
                            _gpio[1].Pins[10].OutputValue = 1;
                            _gpio[1].Pins[20].OutputValue = 0;
                        }
                        break;
                    case LoadState.Unloaded:
                        lock (I2cLock)
                        {
                            _gpio[1].Pins[10].OutputValue = 0;
                            _gpio[1].Pins[20].OutputValue = 1;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Load Socket I2C NG [State : {state}]", ex);
            }
        }
        public void CoverMove(CoverState state)
        {
            try
            {
                if (_gpio == null) throw new InvalidOperationException("GPIO module is not initialized.");

                switch (state)
                {
                    case CoverState.Up:
                        lock (I2cLock)
                        {
                            _gpio[1].Pins[11].OutputValue = 1;
                            _gpio[1].Pins[21].OutputValue = 0;
                        }
                        break;
                    case CoverState.Down:
                        lock (I2cLock)
                        {
                            _gpio[1].Pins[11].OutputValue = 0;
                            _gpio[1].Pins[21].OutputValue = 1;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Cover Move NG [State : {state}]", ex);
            }
        }

        //TODO : Process와 조합하여 동작하는 클래스를 추가로 만들어야됨. 참조 순환 해결해야됨.
        public void PowerOnOff(int port, bool isOn = true)
        {
            try
            {
                if (_gpio == null) throw new InvalidOperationException("GPIO module is not initialized.");

                if (isOn)
                {
                    //STATIC.Process.AddLog(0, $"Power On"); 외부 클래스에다가 사용 - 참조 순환 문제 
                    if (_gpio.Length > 2) { lock (I2cLock) _gpio[2].Pins[9].Direction = 1; }
                    lock (I2cLock) _gpio[1].Pins[9].Direction = 1;
                }
                else
                {
                    //STATIC.Process.AddLog(0, $"Power Off"); 외부 클래스에다가 사용 - 참조 순환 문제 
                    if (_gpio.Length > 2) { lock (I2cLock) _gpio[2].Pins[9].Direction = 0; }
                    lock (I2cLock) _gpio[1].Pins[9].Direction = 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Power On Off I2C NG [OnOff 상태 : {isOn}]", ex);
            }
        }
        public void SetSocketSensor(bool isOn)
        {
            try
            {
                if (_gpio == null) throw new InvalidOperationException("GPIO module is not initialized.");

                if (isOn)
                {
                    lock (I2cLock)
                    {
                        _gpio[1].Pins[12].Enabled = true;
                        _gpio[1].Pins[12].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                        _gpio[1].Pins[12].PulldownEnabled = true;
                        _gpio[1].Pins[13].Enabled = true;
                        _gpio[1].Pins[13].Direction = 0;   //  0 ~ 15 : 0(in), 24 ~ 31 : 1(out)
                        _gpio[1].Pins[13].PulldownEnabled = true;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Socket Sensor I2C NG [OnOff 상태 : {isOn}]", ex);
            }
        }
        public void PowerSequence(int port)
        {
            PowerOnOff(0, false);
            ProcessHelper.Wait(200);
            PowerOnOff(0, true);
            ProcessHelper.Wait(200);
        }
        public bool GetGpioStatus(int input)
        {
            try
            {
                if (_gpio == null) throw new InvalidOperationException("GPIO module is not initialized.");
                lock (I2cLock)
                {
                    if (_gpio[1].Pins[input].Value == 1) return true;
                    else return false;
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"Socket Sensor I2C NG [OnOff 상태 : {input}]", ex);
            }
        }
    }
}
