﻿using System;

public class pcvr
{
		static pcvr _Instance;
		public static pcvr GetInstance()
		{
				if (_Instance == null) {
						_Instance = new pcvr();
						XKOpenCGCamera.GetInstance();
						MyCOMDevice.GetInstance();
				}
				return _Instance;
		}

		// Use this for initialization
		pcvr()
		{
				HID_BUF_LEN = MyCOMDevice.ComThreadClass.BufLenWrite;
				InitJiaoYanMiMa();
		}

		private static int HID_BUF_LEN;
		const byte WriteHead_1 = 0x02;
		const byte WriteHead_2 = 0x55;
		const byte WriteEnd_1 = 0x0d;
		const byte WriteEnd_2 = 0x0a;

		static byte[] JiaoYanMiMa = new byte[4];
		static byte[] JiaoYanMiMaRand = new byte[4];
		void InitJiaoYanMiMa()
		{
				JiaoYanMiMa[1] = 0x8e; //0x8e
				JiaoYanMiMa[2] = 0xc3; //0xc3
				JiaoYanMiMa[3] = 0xd7; //0xd7
				JiaoYanMiMa[0] = 0x00;
				for (int i = 1; i < 4; i++) {
						JiaoYanMiMa[0] ^= JiaoYanMiMa[i];
				}
		}

		void RandomJiaoYanMiMaVal()
		{
				int iSeed = (int)DateTime.Now.ToBinary();
				Random ra = new Random(iSeed);
				for (int i = 0; i < 4; i++) {
						JiaoYanMiMaRand[i] = (byte)ra.Next(0x00, (JiaoYanMiMa[i] - 1));
				}

				byte TmpVal = 0x00;
				for (int i = 1; i < 4; i++) {
						TmpVal ^= JiaoYanMiMaRand[i];
				}

				if (TmpVal == JiaoYanMiMaRand[0]) {
						JiaoYanMiMaRand[0] = JiaoYanMiMaRand[0] == 0x00 ?
								(byte)ra.Next(0x01, 0xff) : (byte)(JiaoYanMiMaRand[0] + ra.Next(0x01, 0xff));
				}
		}

		public void SendMessage()
		{
				if (!MyCOMDevice.IsFindDeviceDt) {
						return;
				}

				byte[] buffer = new byte[HID_BUF_LEN];
				buffer[0] = WriteHead_1;
				buffer[1] = WriteHead_2;
				buffer[HID_BUF_LEN - 2] = WriteEnd_1;
				buffer[HID_BUF_LEN - 1] = WriteEnd_2;

				int iSeed = (int)DateTime.Now.ToBinary();
				Random ra = new Random(iSeed);
				buffer[6] =  (byte)ra.Next(0, 253);

				byte jiGuangQiCount = CSampleGrabberCB.IndexMousePoint;
				//buffer[7]: 0 -> 激光器P1,  1 -> 激光器P2.
				switch (CSampleGrabberCB.m_mode) {
				case MODE.MODE_MOTION:
						if (jiGuangQiCount % CSampleGrabberCB.JiGuangLQ == 0) {
								buffer[7] = (byte)(0x01 << (jiGuangQiCount / CSampleGrabberCB.JiGuangLQ));
						}
						else {
								//用于冷却关闭所有激光器,确保摄像机画面同一时刻只有一个激光点.
								buffer[7] = 0x00;
						}
						//ScreenLog.Log("IndexMousePoint *** "+CSampleGrabberCB.IndexMousePoint);
						break;
				case MODE.MODE_SET_CALIBRATION:
						buffer[7] = 0xFF;
						break;
				}
				MyCOMDevice.ComThreadClass.WriteByteMsg = buffer;

//				byte[] bufferTmp = {0x02, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x59, 0x00, 0xd1, 0xbe, 0x2c, 0xdb,
//						0xb5, 0x82, 0x4d, 0x03, 0x59, 0x39, 0x4a, 0x6e, 0x80, 0x06, 0x26, 0x0a, 0x2a, 0x2c, 0x0d, 0x0a};
//				MyCOMDevice.ComThreadClass.WriteByteMsg = bufferTmp;
		}
}