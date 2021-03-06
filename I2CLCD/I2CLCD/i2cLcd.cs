// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
// PB avec carte NetDuino lors de l'envoi de plusieurs transactions en �criture
// A partir de la deuxi�me, les deux premiers octet du buffer ne sont pas pris en compte !
// Solution : rajout de deux octets morts !
// Surveiller les prochaines versions du SDK 
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
using System;
using System.Threading;
using Microsoft.SPOT.Hardware;

namespace testMicroToolsKit
{
    namespace Hardware
    {
        namespace Displays
        {

            public class I2CLcd
            {
                private I2CDevice.Configuration ConfigI2CLcd;
                private I2CDevice BusI2C;
                private ushort i2c_Add_7bits = 0x3A;

                public enum LcdManufacturer : byte
                {
                    MIDAS = 0x3A,
                    BATRON = 0x3B
                }

                public enum CursorType : byte
                {
                    Hide,
                    Underline,
                    Blink
                };

                /// <summary>
                /// This constructor assumes the default Midas factory Slave Address = 0x3A and bus frequency = 100kHz
                /// </summary>
                public I2CLcd()
                {
                    ConfigI2CLcd = new I2CDevice.Configuration(i2c_Add_7bits, 100);
                }

                /// <summary>
                /// This constructor allows user to specify the Slave Address, bus frequency = 100khz
                /// </summary>
                /// <param name="I2C_Add_7bits">Manufacturer Adress I2C</param>
                public I2CLcd(ushort I2C_Add_7bits)
                {
                    this.i2c_Add_7bits = I2C_Add_7bits;
                    ConfigI2CLcd = new I2CDevice.Configuration(i2c_Add_7bits, 100);
                }

                /// <summary>
                /// This constructor allows user to specify the LCD Manufacturer name, bus frequency = 100khz
                /// </summary>
                /// <param name="ManufacturerName">LCD Manufacturer Name</param>
                public I2CLcd(LcdManufacturer ManufacturerName)
                {
                    this.i2c_Add_7bits = (ushort)ManufacturerName;
                    ConfigI2CLcd = new I2CDevice.Configuration(i2c_Add_7bits, 100);
                }

                /// <summary>
                /// This constructor allows user to specify the Slave Address and bus frequency
                /// </summary>
                /// <param name="I2C_Add_7bits">Manufacturer Adress I2C</param>
                /// <param name="FreqBusI2C">Bus frequency</param>
                public I2CLcd(ushort I2C_Add_7bits, Int16 FreqBusI2C)
                {
                    this.i2c_Add_7bits = I2C_Add_7bits;
                    ConfigI2CLcd = new I2CDevice.Configuration(i2c_Add_7bits, FreqBusI2C = 100);
                }

                /// <summary>
                /// This constructor allows user to specify the LCD Manufacturer name and bus frequency
                /// </summary>
                ///<param name="ManufacturerName">LCD Manufacturer Name</param>
                /// <param name="FreqBusI2C">Bus frequency</param>
                public I2CLcd(LcdManufacturer ManufacturerName, Int16 FreqBusI2C = 100)
                {
                    this.i2c_Add_7bits = (ushort)ManufacturerName;
                    ConfigI2CLcd = new I2CDevice.Configuration(i2c_Add_7bits, FreqBusI2C);
                }

                /// <summary>
                /// Initialize the LCD before use 
                /// </summary>
                public void Init()
                {
                    Thread.Sleep(200);
                    // Nop, Function_set, Display_ctl, Entry_mode_set, Function_set, Disp_conf, Temp_ctl, Hv_gen, VLCD_Set, Function_Set, Set DDRAM, Return Home
                    byte[] outbuffer = new byte[] { 0x00, 0x34, 0x0d, 0x06, 0x35, 0x05, 0x10, 0x40, 0x99, 0x34, 0x83, 0x02 };

                    if (i2c_Add_7bits == (ushort)LcdManufacturer.BATRON)
                    {
                        outbuffer[5] = 0x04; // Disp_conf
                        outbuffer[7] = 0x42; // Hv_gen
                        outbuffer[8] = 0x9F; // VLCD_Set
                    }

                    I2CDevice.I2CTransaction WriteTransaction = I2CDevice.CreateWriteTransaction(outbuffer);
                    I2CDevice.I2CTransaction[] T_WriteBytes = new I2CDevice.I2CTransaction[] { WriteTransaction };
                    BusI2C = new I2CDevice(ConfigI2CLcd); 
                    BusI2C.Execute(T_WriteBytes, 1000);
                    BusI2C.Dispose();
                }

                /// <summary>
                /// Three cursor modes
                /// </summary>
                /// <param name="posCursor"></param>
                public void SelectCursor(CursorType posCursor)
                {
                    byte[] outbuffer = new byte[] { 0, (byte)(4 + posCursor) };
                    I2CDevice.I2CTransaction WriteTransaction = I2CDevice.CreateWriteTransaction(outbuffer);
                    I2CDevice.I2CTransaction[] T_WriteBytes = new I2CDevice.I2CTransaction[] { WriteTransaction };
                    BusI2C = new I2CDevice(ConfigI2CLcd);
                    BusI2C.Execute(T_WriteBytes, 1000);
                    BusI2C.Dispose();               
                }

                /// <summary>
                /// Set brightness  20 dark -> 0 light
                /// </summary>
                /// <param name="bright"></param>
                public void SetBacklight(byte bright)
                {
                    byte BLight = 0x99;

                    if (i2c_Add_7bits == (ushort)LcdManufacturer.MIDAS)
                    {
                        if (bright > 20)
                        {
                            bright = 20;
                        }
                        BLight = (byte)(0x90 + (byte)(bright & 0x1F));
                    }
                    else if (i2c_Add_7bits == (ushort)LcdManufacturer.BATRON)
                    {
                        if (bright > 20)
                        {
                            bright = 20;
                        }
                        BLight = (byte)(0x95 + (byte)(bright & 0x1F));
                    }
  
                    byte[] outbuffer = new byte[] { 0, 0x35, BLight, 0x34 };
                    I2CDevice.I2CTransaction WriteTransaction = I2CDevice.CreateWriteTransaction(outbuffer);
                    I2CDevice.I2CTransaction[] T_WriteBytes = new I2CDevice.I2CTransaction[] { WriteTransaction };
                    BusI2C = new I2CDevice(ConfigI2CLcd);
                    BusI2C.Execute(T_WriteBytes, 1000);
                    BusI2C.Dispose(); 
                }

                /// <summary>
                /// Goto start of line
                /// </summary>
                /// <param name="x"></param>
                public void LineBegin(byte x)
                {
                    byte addr = 0x80;
                    if (x == 1) addr += 0x40;
                    byte[] outbuffer = new byte[] { 0, addr };
                    I2CDevice.I2CTransaction WriteTransaction = I2CDevice.CreateWriteTransaction(outbuffer);
                    I2CDevice.I2CTransaction[] T_WriteBytes = new I2CDevice.I2CTransaction[] { WriteTransaction };
                    BusI2C = new I2CDevice(ConfigI2CLcd);
                    BusI2C.Execute(T_WriteBytes, 1000);
                    BusI2C.Dispose();
                }

                /// <summary>
                /// Write a line of text at x,y
                /// </summary>
                /// <param name="x_pos"></param>
                /// <param name="y_pos"></param>
                /// <param name="Text"></param>
                public void PutString(byte x_pos, byte y_pos, string Text)
                {

                    byte addr = 0x80;
                    if (x_pos < 17) addr += x_pos;// This is for 16 x 2, adjust as nessesary
                    if (y_pos == 1) addr += 0x40;
                    byte[] txt = System.Text.Encoding.UTF8.GetBytes((byte)'0' + (byte)'0' + (byte)'0' + Text);
                    for (byte z = 3; z < (Text.Length + 3); z++) // All characters have to be moved up!!
                    { txt[z] += 128; }
                    txt[2] = 0x40;  // R/S to Data

                    byte[] outbuffer = new byte[] { 0, addr };
                    I2CDevice.I2CTransaction WriteBytes = I2CDevice.CreateWriteTransaction(outbuffer); // set address
                    I2CDevice.I2CTransaction WriteText = I2CDevice.CreateWriteTransaction(txt); // Print line                                                                                               
                    I2CDevice.I2CTransaction[] T_WriteBytes = new I2CDevice.I2CTransaction[] { WriteBytes, WriteText };                  
                    BusI2C = new I2CDevice(ConfigI2CLcd);
                    BusI2C.Execute(T_WriteBytes, 1000);
                    BusI2C.Dispose();  
                }

                /// <summary>
                /// Single character write at x,y
                /// </summary>
                /// <param name="x_pos"></param>
                /// <param name="y_pos"></param>
                /// <param name="z_char"></param>
                public void PutChar(byte x_pos, byte y_pos, byte z_char)
                {
                    byte addr = 0x80;                 // This is for 16 x 2, adjust as nessesary
                    if (x_pos < 17) addr += x_pos;    // x dir
                    if (y_pos == 1) addr += 0x40;     // y dir
                    byte[] outbuffer = new byte[] { 0, addr };
                    I2CDevice.I2CTransaction WriteBytes = I2CDevice.CreateWriteTransaction(outbuffer); // set address (R/S to Cmd )
                    I2CDevice.I2CTransaction WriteText = I2CDevice.CreateWriteTransaction(new byte[] { 0, 0, 0x40, z_char });  // single char write
                                                                                                                               // Tableaux des transactions 
                    I2CDevice.I2CTransaction[] T_WriteBytes = new I2CDevice.I2CTransaction[] { WriteBytes, WriteText };
                    BusI2C = new I2CDevice(ConfigI2CLcd); 
                    BusI2C.Execute(T_WriteBytes, 1000);
                    BusI2C.Dispose();  
                }

                // Clear screen with 0xA0 not 0x20
                /// <summary>
                /// Clear screen
                /// </summary>
                public void ClearScreen()                     // If you use built in Clr
                {                                             // the screen doen't clear
                    byte[] clr = new byte[19];                // you just get arrows
                    clr[0] = 0; clr[1] = 0; clr[2] = 0x40;    // R/S to Data
                    for (byte x = 3; x < 19; x++)             // Space is not 0x20 on this unit!
                    { clr[x] = 0xA0; }
                    I2CDevice.I2CTransaction SetTop = I2CDevice.CreateWriteTransaction(new byte[] { 0, 0x80 }); // set top line (R/S to Cmd )
                    I2CDevice.I2CTransaction Setclr = I2CDevice.CreateWriteTransaction(clr);
                    I2CDevice.I2CTransaction SetBottom = I2CDevice.CreateWriteTransaction(new byte[] { 0, 0, 0, 0xC0 });  // set bottom line (R/S to Cmd )
                    I2CDevice.I2CTransaction SetHome = I2CDevice.CreateWriteTransaction(new byte[] { 0, 0, 0, 0x80 }); // Cursor to home (R/S to Cmd )
                                                                                                                       // Tableaux des transactions 
                    I2CDevice.I2CTransaction[] T_WriteBytes = new I2CDevice.I2CTransaction[] { SetTop, Setclr, SetBottom, Setclr, SetHome };
                    BusI2C = new I2CDevice(ConfigI2CLcd);
                    BusI2C.Execute(T_WriteBytes, 1000);
                    BusI2C.Dispose(); 
                }
            }
        }
    }
}
