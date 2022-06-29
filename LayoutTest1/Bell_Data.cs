using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;



namespace LayoutTest1
{
    class Bell_Data
    {

        // header================================
        public uint Version;
        public uint Flags;
        public uint Length;

        
        // basic data ===========================
        public uint Option; //공통 basic data
        
        public uint Result;
        public uint Checksum; //basic data 1

        public uint Model_no;
        public uint Firmware_ver; //basic data 2


        // Data =================================
        public uint[] Data = new uint[32];
        public uint Bell_ID = 0;
        public uint button_info;
        public uint bell_info;
        public uint reserved;
        





        public Bell_Data(SerialPort sp)
        {
            //initialize 

            this.Version = (uint)sp.ReadByte();
            this.Flags = (uint)sp.ReadByte();
            this.Length = (uint)(sp.ReadByte() << 8 ) + (uint)sp.ReadByte();
            this.Option = (uint)(sp.ReadByte() << 8 ) + (uint)sp.ReadByte();
            switch(Option)
            {
                case 0xA501: //PC 연결 확인 요청 
                    this.Result = (uint)sp.ReadByte();
                    this.Checksum = (uint)sp.ReadByte();
                    for(int i = 0; i < this.Length - 8 ;i++)
                    {
                        Data[i] = (uint)sp.ReadByte();
                    }
                    break;

                case 0xA505: //Bell Data 발생 요청
                    this.Model_no = (uint)(sp.ReadByte() << 8) + (uint)sp.ReadByte();
                    this.Firmware_ver = (uint)(sp.ReadByte() << 8) + (uint)sp.ReadByte();
                    for(int j = 0 ; j < 4 ; j++)
                    {
                        this.Bell_ID = (this.Bell_ID << 8) + (uint)sp.ReadByte();
                    }
                    this.button_info = (uint)sp.ReadByte();
                    this.bell_info = (uint)sp.ReadByte();
                    for(int k = 0 ; k < 3 ; k++)
                    {
                        //reserved
                        this.reserved = (uint)(this.reserved << 8) + (uint)sp.ReadByte();
                    }
                    if (this.reserved != 0)
                    {
                        Console.WriteLine("some data received from reserved data");
                    }
                    this.Checksum = (uint)sp.ReadByte();
                    break;
            }
        }

        public bool check_checksum()
        {
            uint sum = 0;
            if (this.Option == 0xA501)
            {
                sum += this.Version;
                sum += this.Flags;
                sum += ((this.Length & 0xFF00) >> 8) + (this.Length & 0xFF);
                sum += ((this.Option & 0xFF00) >> 8) + (this.Option & 0xFF);
                sum += this.Result;

                for(int i = 0 ; i < this.Data.Length ; i++)
                {
                    sum += this.Data[i];
                }
                uint result = twos_complement(sum);

                if (result == this.Checksum)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(this.Option == 0xA505)
            {
                sum += this.Version;
                sum += this.Flags;
                sum += ((this.Length & 0xFF00) >> 8) + (this.Length & 0xFF);
                sum += ((this.Option & 0xFF00) >> 8) + (this.Option & 0xFF);
                sum += ((this.Model_no & 0xFF00) >> 8) + (this.Model_no & 0xFF);
                sum += ((this.Firmware_ver & 0xFF00) >> 8) + (this.Firmware_ver & 0xFF);
                
                uint bell_id_ = this.Bell_ID ;
                uint reserved_ = this.reserved;
                for (int j = 0 ; j < 4 ; j++)
                {
                    //bell_id
                    sum += bell_id_ & 0xFF;
                    bell_id_ >>= 8;
                }
                sum += this.button_info;
                sum += this.bell_info;
                for (int k = 0; k < 3; k++)
                {
                    //reserved
                    sum += reserved_ & 0xFF;
                    reserved_ >>= 8;
                }

                uint result = twos_complement(sum);

                if (result == this.Checksum)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
                Console.WriteLine("checksum을 계산하지 못했습니다.");
            }
        }

        private uint twos_complement(uint sum)
        {
            sum = sum & 0xFF; //캐리 제거
            uint result = (uint)~sum; //unsigned 형식으로 1의 보수 계산
            result  = result & 0xFF; // 앞에 생기는 1111 1111 1111 등의 자릿수 제거
            result += 1; // 1을 더해 2의 보수를 만듬
            return result;
        }


    }
}
