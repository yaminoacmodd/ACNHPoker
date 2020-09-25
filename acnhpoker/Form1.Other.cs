﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class Form1 : Form
    {
        private void UpdateTownID()
        {
            byte[] townID = Utilities.GetTownID(s, bot);
            /*TownName.Text = String.Format("{1:X2} {2:X2} {3:X2} {4:X2} | {0}",
                Encoding.Unicode.GetString(townID, 4, 0x18),
                townID[3], townID[2], townID[1], townID[0]);*/
            //TownName.Text = Encoding.Unicode.GetString(townID, 4, 0x18);
            this.Text = this.Text + "  |  Island Name : " + Encoding.Unicode.GetString(townID, 4, 0x18);
        }

        private void readWeatherSeed()
        {
            byte[] b = Utilities.GetWeatherSeed(s, bot);
            string result = Utilities.ByteToHexString(b);
            UInt32 decValue = Convert.ToUInt32(Utilities.flip(result), 16);
            UInt32 Seed = decValue - 2147483648;
            SeedTextbox.Text = Seed.ToString();
        }

        private void eatBtn_Click(object sender, EventArgs e)
        {
            Utilities.setStamina(s, bot, "0A");
            System.Media.SystemSounds.Asterisk.Play();
            setEatBtn();
        }

        private void poopBtn_Click(object sender, EventArgs e)
        {
            Utilities.setStamina(s, bot, "00");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void setEatBtn()
        {
            eatBtn.Visible = true;
            Random rnd = new Random();
            int dice = rnd.Next(1, 8);

            switch (dice)
            {
                case 1:
                    eatBtn.Text = "Eat 10 Apples";
                    break;
                case 2:
                    eatBtn.Text = "Eat 10 Oranges";
                    break;
                case 3:
                    eatBtn.Text = "Eat 10 Cherries";
                    break;
                case 4:
                    eatBtn.Text = "Eat 10 Pears";
                    break;
                case 5:
                    eatBtn.Text = "Eat 10 Peaches";
                    break;
                case 6:
                    eatBtn.Text = "Eat 10 Coconuts";
                    break;
                default:
                    eatBtn.Text = "Eat 10 Turnips";
                    break;
            }
        }

        private void selectedItem_Click(object sender, EventArgs e)
        {
            if (selectedButton == null)
            {
                int Slot = findEmpty();
                if (Slot > 0)
                {
                    selectedSlot = Slot;
                    updateSlot(Slot);
                }
            }

            if (currentPanel == itemModePanel)
            {
                customIdBtn_Click(sender, e);
            }
            else if (currentPanel == recipeModePanel)
            {
                spawnRecipeBtn_Click(sender, e);
            }
            else if (currentPanel == flowerModePanel)
            {
                spawnFlowerBtn_Click(sender, e);
            }
            int firstSlot = findEmpty();
            if (firstSlot > 0)
            {
                selectedSlot = firstSlot;
                updateSlot(firstSlot);
            }
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void updateSlot(int select)
        {
            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                if (btn.Tag == null)
                    continue;
                btn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
                if (int.Parse(btn.Tag.ToString()) == select)
                {
                    selectedButton = btn;
                    btn.BackColor = System.Drawing.Color.LightSeaGreen;
                }
            }
        }

        public void updateSelectedItemInfo(string Name, string ID, string Data)
        {
            selectedItemName.Text = Name;
            selectedID.Text = ID;
            selectedData.Text = Data;
            selectedFlag1.Text = selectedItem.getFlag1();
            selectedFlag2.Text = selectedItem.getFlag2();
        }

        public string GetNameFromID(string itemID, DataTable source)
        {
            if (source == null)
            {
                return "";
            }

            DataRow row = source.Rows.Find(itemID);

            if (row == null)
            {
                return ""; //row not found
            }
            else
            {
                //row found set the index and find the name
                return (string)row[0];
            }

        }

        public string GetImagePathFromID(string itemID, DataTable source, UInt32 data = 0)
        {
            if (source == null)
            {
                return "";
            }

            DataRow row = source.Rows.Find(itemID);
            DataRow VarRow = variationSource.Rows.Find(itemID);

            if (row == null)
            {
                return ""; //row not found
            }
            else
            {

                string path;
                if (VarRow != null & source != recipeSource)
                {
                    path = @"img\variation\" + VarRow[1] + @"\" + VarRow[3] + ".png";
                    if (File.Exists(path))
                    {
                        return path;
                    }
                    string main = (data & 0xF).ToString();
                    string sub = (((data & 0xFF) - (data & 0xF)) / 0x20).ToString();
                    //Debug.Print("data " + data.ToString("X") + " Main " + main + " Sub " + sub);
                    path = @"img\variation\" + VarRow[1] + @"\" + VarRow[3] + "_" + main + "_" + sub + ".png";
                    if (File.Exists(path))
                    {
                        return path;
                    }

                }

                path = @"img\" + row[1] + @"\" + row[0] + ".png";
                if (File.Exists(path))
                {
                    return path; //file found
                }
                else
                {
                    return ""; //file not found
                }
            }
        }

        private void copyItemBtn_Click(object sender, EventArgs e)
        {
            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                if (item.Owner is ContextMenuStrip owner)
                {
                    itemModeBtn_Click(sender, e);
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        hexMode_Click(sender, e);
                    }
                    var btn = (inventorySlot)owner.SourceControl;
                    selectedItem.setup(btn);
                    if (selection != null)
                    {
                        selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4));
                    }
                    updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    if (selectedItem.fillItemID() == "FFFE")
                    {
                        hexMode_Click(sender, e);
                        customAmountTxt.Text = "";
                        customIdTextbox.Text = "";
                    }
                    else
                    {
                        customAmountTxt.Text = Utilities.precedingZeros(selectedItem.fillItemData(), 8);
                        customIdTextbox.Text = Utilities.precedingZeros(selectedItem.fillItemID(), 4);
                    }
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        private void wrapItemBtn_Click(object sender, EventArgs e)
        {
            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                if (item.Owner is ContextMenuStrip owner)
                {
                    string flag = "00";
                    if (RetainNameCheckBox.Checked)
                    {
                        switch (wrapSetting.SelectedIndex)
                        {
                            case 0:
                                flag = "41";
                                break;

                            case 1:
                                flag = "45";
                                break;

                            case 2:
                                flag = "49";
                                break;

                            case 3:
                                flag = "4D";
                                break;

                            case 4:
                                flag = "51";
                                break;

                            case 5:
                                flag = "55";
                                break;

                            case 6:
                                flag = "59";
                                break;

                            case 7:
                                flag = "5D";
                                break;

                            case 8:
                                flag = "61";
                                break;

                            case 9:
                                flag = "65";
                                break;

                            case 10:
                                flag = "69";
                                break;

                            case 11:
                                flag = "6D";
                                break;

                            case 12:
                                flag = "71";
                                break;

                            case 13:
                                flag = "75";
                                break;

                            case 14:
                                flag = "79";
                                break;

                            case 15:
                                flag = "7D";
                                break;

                            case 16:
                                flag = "42";
                                break;

                            case 17:
                                flag = "43";
                                break;
                        }
                    }
                    else
                    {
                        switch (wrapSetting.SelectedIndex)
                        {
                            case 0:
                                flag = "01";
                                break;

                            case 1:
                                flag = "05";
                                break;

                            case 2:
                                flag = "09";
                                break;

                            case 3:
                                flag = "0D";
                                break;

                            case 4:
                                flag = "11";
                                break;

                            case 5:
                                flag = "15";
                                break;

                            case 6:
                                flag = "19";
                                break;

                            case 7:
                                flag = "1D";
                                break;

                            case 8:
                                flag = "21";
                                break;

                            case 9:
                                flag = "25";
                                break;

                            case 10:
                                flag = "29";
                                break;

                            case 11:
                                flag = "2D";
                                break;

                            case 12:
                                flag = "31";
                                break;

                            case 13:
                                flag = "35";
                                break;

                            case 14:
                                flag = "39";
                                break;

                            case 15:
                                flag = "3D";
                                break;

                            case 16:
                                flag = "02";
                                break;

                            case 17:
                                flag = "03";
                                break;
                        }
                    }

                    if (!offline)
                    {
                        byte[] Bank01to20 = Utilities.GetInventoryBank(s, bot, 1);
                        byte[] Bank21to40 = Utilities.GetInventoryBank(s, bot, 21);

                        int slot = int.Parse(owner.SourceControl.Tag.ToString());
                        byte[] slotBytes = new byte[2];

                        int slotOffset;
                        if (slot < 21)
                        {
                            slotOffset = ((slot - 1) * 0x8);
                        }
                        else
                        {
                            slotOffset = ((slot - 21) * 0x8);
                        }

                        if (slot < 21)
                        {
                            Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x2);
                        }
                        else
                        {
                            Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x2);
                        }

                        string slotID = Utilities.flip(Utilities.ByteToHexString(slotBytes));

                        if (slotID == "FFFE")
                        {
                            return;
                        }

                        Utilities.setFlag1(s, bot, slot, flag);

                        var btnParent = (inventorySlot)owner.SourceControl;
                        btnParent.setFlag1(flag);
                        btnParent.refresh(false);
                    }
                    else
                    {
                        var btnParent = (inventorySlot)owner.SourceControl;
                        if (btnParent.fillItemID() != "FFFE")
                        {
                            btnParent.setFlag1(flag);
                            btnParent.refresh(false);
                        }
                    }

                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        private void hexMode_Click(object sender, EventArgs e)
        {
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                AmountLabel.Text = "Hex Value";
                hexModeBtn.Tag = "Hex";
                hexModeBtn.Text = "Normal Mode";
                if (customAmountTxt.Text != "")
                {
                    int decValue = int.Parse(customAmountTxt.Text) - 1;
                    string hexValue;
                    if (decValue < 0)
                        hexValue = "0";
                    else
                        hexValue = decValue.ToString("X");
                    customAmountTxt.Text = Utilities.precedingZeros(hexValue, 8);
                }
            }
            else
            {
                AmountLabel.Text = "Amount";
                hexModeBtn.Tag = "Normal";
                hexModeBtn.Text = "Hex Mode";
                if (customAmountTxt.Text != "")
                {
                    string hexValue = customAmountTxt.Text;
                    int decValue = Convert.ToInt32(hexValue, 16) + 1;
                    customAmountTxt.Text = decValue.ToString();
                }
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            /*
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to save your inventory?\n[Warning] Your previous save will be overwritten!", "Load inventory", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {*/
            try
            {
                SaveFileDialog file = new SaveFileDialog()
                {
                    Filter = "New Horizons Inventory (*.nhi)|*.nhi",
                    //FileName = "items.nhi",
                };

                string savepath = Directory.GetCurrentDirectory() + @"\save";
                //Debug.Print(savepath);
                if (Directory.Exists(savepath))
                {
                    file.InitialDirectory = savepath;
                }
                else
                {
                    file.InitialDirectory = @"C:\";
                }

                if (file.ShowDialog() != DialogResult.OK)
                    return;

                string Bank = "";

                if (!offline)
                {
                    byte[] Bank01to20 = Utilities.GetInventoryBank(s, bot, 1);
                    byte[] Bank21to40 = Utilities.GetInventoryBank(s, bot, 21);
                    Bank = Utilities.ByteToHexString(Bank01to20) + Utilities.ByteToHexString(Bank21to40);


                    byte[] save = new byte[320];

                    Array.Copy(Bank01to20, 0, save, 0, 160);
                    Array.Copy(Bank21to40, 0, save, 160, 160);

                    File.WriteAllBytes(file.FileName, save);
                }
                else
                {
                    inventorySlot[] SlotPointer = new inventorySlot[40];
                    foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                    {
                        int slotId = int.Parse(btn.Tag.ToString());
                        SlotPointer[slotId - 1] = btn;
                    }
                    for (int i = 0; i < SlotPointer.Length; i++)
                    {
                        string first = Utilities.flip(Utilities.precedingZeros(SlotPointer[i].getFlag1() + SlotPointer[i].getFlag2() + Utilities.precedingZeros(SlotPointer[i].fillItemID(), 4), 8));
                        string second = Utilities.flip(Utilities.precedingZeros(SlotPointer[i].fillItemData(), 8));
                        //Debug.Print(first + " " + second + " " + SlotPointer[i].getFlag1() + " " + SlotPointer[i].getFlag2() + " " + SlotPointer[i].fillItemID());
                        Bank = Bank + first + second;
                    }

                    byte[] save = new byte[320];

                    for (int i = 0; i < Bank.Length / 2 - 1; i++)
                    {
                        string data = String.Concat(Bank[(i * 2)].ToString(), Bank[((i * 2) + 1)].ToString());
                        //Debug.Print(i.ToString() + " " + data);
                        save[i] = Convert.ToByte(data, 16);
                    }

                    File.WriteAllBytes(file.FileName, save);
                }
                //Debug.Print(Bank);

                System.Media.SystemSounds.Asterisk.Play();
                /*
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                config.AppSettings.Settings["save01"].Value = Bank1.Substring(0, 320);
                config.AppSettings.Settings["save21"].Value = Bank2.Substring(0, 320);
                config.Save(ConfigurationSaveMode.Minimal);
                MessageBox.Show("Inventory Saved!");
                */
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                return;
            }
            //}
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            /*DialogResult dialogResult = MessageBox.Show("Are you sure you want to load your inventory?\n[Warning] All of your inventory slots will be overwritten!", "Load inventory", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {*/
            try
            {
                /*
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                string Bank1 = config.AppSettings.Settings["save01"].Value;
                string Bank2 = config.AppSettings.Settings["save21"].Value;
                byte[] Bank01to20 = Encoding.ASCII.GetBytes(Bank1);
                byte[] Bank21to40 = Encoding.ASCII.GetBytes(Bank2);

                Thread LoadThread = new Thread(delegate () { loadInventory(Bank01to20, Bank21to40); });
                LoadThread.Start();
                */

                OpenFileDialog file = new OpenFileDialog()
                {
                    Filter = "New Horizons Inventory (*.nhi)|*.nhi|All files (*.*)|*.*",
                    FileName = "items.nhi",
                };

                string savepath = Directory.GetCurrentDirectory() + @"\save";
                //Debug.Print(savepath);
                if (Directory.Exists(savepath))
                {
                    file.InitialDirectory = savepath;
                }
                else
                {
                    file.InitialDirectory = @"C:\";
                }

                if (file.ShowDialog() != DialogResult.OK)
                    return;

                byte[] data = File.ReadAllBytes(file.FileName);

                btnToolTip.RemoveAll();
                /*
                string bank = "";
                for (int i = 0; i < data.Length; i++)
                {
                    bank += Utilities.precedingZeros(((UInt16)data[i]).ToString("X"), 2);
                }
                */
                //Debug.Print(bank);
                Thread LoadThread = new Thread(delegate () { loadInventory(data); });
                LoadThread.Start();
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                return;
            }

            //}
        }

        private void loadInventory(byte[] data)
        {
            showWait();

            if (!offline)
            {
                byte[] b1 = new byte[160];
                byte[] b2 = new byte[160];

                for (int i = 0; i < b1.Length; i++)
                {
                    b1[i] = data[i];
                    b2[i] = data[i + 160];
                }

                Utilities.OverwriteAll(s, bot, b1, b2, ref counter);
                /*
                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    if (btn.Tag == null)
                        continue;

                    if (btn.Tag.ToString() == "")
                        continue;

                    int slotId = int.Parse(btn.Tag.ToString());

                    int slotOffset = 0;
                    int countOffset = 0;

                    slotOffset = ((slotId - 1) * 16);
                    countOffset = ((slotId - 1) * 16) + 8;

                    string itemID = "";
                    string itemData = "";

                    string slotBytes = bank.Substring(slotOffset, 8);
                    string dataBytes = bank.Substring(countOffset, 8);


                    itemID = Utilities.flip(slotBytes);
                    itemData = Utilities.flip(dataBytes);

                    Utilities.SpawnItem(s, bot, slotId, itemID, itemData);

                }
                */

                /*
                Invoke((MethodInvoker)delegate
                {
                    UpdateInventory();
                });
                */
            }

            Invoke((MethodInvoker)delegate
            {
                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    if (btn.Tag == null)
                        continue;

                    if (btn.Tag.ToString() == "")
                        continue;

                    int slotId = int.Parse(btn.Tag.ToString());

                    byte[] slotBytes = new byte[2];
                    byte[] flag1Bytes = new byte[1];
                    byte[] flag2Bytes = new byte[1];
                    byte[] dataBytes = new byte[4];
                    byte[] recipeBytes = new byte[2];

                    int slotOffset = ((slotId - 1) * 0x8);
                    int flag1Offset = 0x3 + ((slotId - 1) * 0x8);
                    int flag2Offset = 0x2 + ((slotId - 1) * 0x8);
                    int countOffset = 0x4 + ((slotId - 1) * 0x8);

                    Buffer.BlockCopy(data, slotOffset, slotBytes, 0, 2);
                    Buffer.BlockCopy(data, flag1Offset, flag1Bytes, 0, 1);
                    Buffer.BlockCopy(data, flag2Offset, flag2Bytes, 0, 1);
                    Buffer.BlockCopy(data, countOffset, dataBytes, 0, 4);
                    Buffer.BlockCopy(data, countOffset, recipeBytes, 0, 2);

                    string itemID = Utilities.flip(Utilities.ByteToHexString(slotBytes));
                    string itemData = Utilities.flip(Utilities.ByteToHexString(dataBytes));
                    string recipeData = Utilities.flip(Utilities.ByteToHexString(recipeBytes));
                    string flag1 = Utilities.ByteToHexString(flag1Bytes);
                    string flag2 = Utilities.ByteToHexString(flag2Bytes);

                    //Debug.Print("Slot : " + slotId.ToString() + " ID : " + itemID + " Data : " + itemData + " recipeData : " + recipeData + " Flag1 : " + flag1 + " Flag2 : " + flag2);

                    if (itemID == "FFFE") //Nothing
                    {
                        btn.setup("", 0xFFFE, 0x0, "", "00", "00");
                        continue;
                    }
                    else if (itemID == "16A2") //Recipe
                    {
                        btn.setup(GetNameFromID(recipeData, recipeSource), 0x16A2, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, recipeSource), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "1095") //Delivery
                    {
                        btn.setup(GetNameFromID(recipeData, itemSource), 0x1095, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, itemSource, Convert.ToUInt32("0x" + itemData, 16)), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "16A1") //Bottle Message
                    {
                        btn.setup(GetNameFromID(recipeData, recipeSource), 0x16A1, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, recipeSource), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "0A13") // Fossil
                    {
                        btn.setup(GetNameFromID(recipeData, itemSource), 0x0A13, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, itemSource), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "114A") // Money Tree
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemData, 16)), GetNameFromID(recipeData, itemSource), flag1, flag2);
                        continue;
                    }
                    else
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemData, 16)), "", flag1, flag2);
                        continue;
                    }
                }
            });

            hideWait();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private int findEmpty()
        {
            /*
            byte[] Bank01to20 = Utilities.GetInventoryBank(s, bot, 1);
            byte[] Bank21to40 = Utilities.GetInventoryBank(s, bot, 21);
            //string Bank1 = Encoding.ASCII.GetString(Bank01to20);
            //string Bank2 = Encoding.ASCII.GetString(Bank21to40);
            //Debug.Print(Bank1);
            //Debug.Print(Bank2);

            if (Bank01to20 == null | Bank21to40 == null)
            {
                return -1;
            }

            for (int slot = 1; slot <= 40; slot++)
            {
                byte[] slotBytes = new byte[4];

                int slotOffset;
                if (slot < 21)
                {
                    slotOffset = ((slot - 1) * 0x10);
                }
                else
                {
                    slotOffset = ((slot - 21) * 0x10);
                }

                if (slot < 21)
                {
                    Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x4);
                }
                else
                {
                    Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x4);
                }

                string itemID = Utilities.flip(Encoding.ASCII.GetString(slotBytes));

                if (itemID == "FFFE")
                {
                    return slot;
                }
            }
            */
            inventorySlot[] SlotPointer = new inventorySlot[40];

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                int slotId = int.Parse(btn.Tag.ToString());
                SlotPointer[slotId - 1] = btn;
            }
            for (int i = 0; i < SlotPointer.Length; i++)
            {
                if (SlotPointer[i].fillItemID() == "FFFE")
                    return i + 1;
            }

            return -1;
        }

        private void UpdateTurnipPrices()
        {
            UInt32[] turnipPrices = Utilities.GetTurnipPrices(s, bot);
            turnipBuyPrice.Clear();
            turnipBuyPrice.SelectionAlignment = HorizontalAlignment.Center;
            turnipBuyPrice.Text = String.Format("{0}", turnipPrices[12]);
            int buyPrice = int.Parse(String.Format("{0}", turnipPrices[12]));

            turnipSell1AM.Clear();
            turnipSell1AM.Text = String.Format("{0}", turnipPrices[0]);
            int MondayAM = int.Parse(String.Format("{0}", turnipPrices[0]));
            setTurnipColor(buyPrice, MondayAM, turnipSell1AM);

            turnipSell1PM.Clear();
            turnipSell1PM.Text = String.Format("{0}", turnipPrices[1]);
            int MondayPM = int.Parse(String.Format("{0}", turnipPrices[1]));
            setTurnipColor(buyPrice, MondayPM, turnipSell1PM);

            turnipSell2AM.Clear();
            turnipSell2AM.Text = String.Format("{0}", turnipPrices[2]);
            int TuesdayAM = int.Parse(String.Format("{0}", turnipPrices[2]));
            setTurnipColor(buyPrice, TuesdayAM, turnipSell2AM);

            turnipSell2PM.Clear();
            turnipSell2PM.Text = String.Format("{0}", turnipPrices[3]);
            int TuesdayPM = int.Parse(String.Format("{0}", turnipPrices[3]));
            setTurnipColor(buyPrice, TuesdayPM, turnipSell2PM);

            turnipSell3AM.Clear();
            turnipSell3AM.Text = String.Format("{0}", turnipPrices[4]);
            int WednesdayAM = int.Parse(String.Format("{0}", turnipPrices[4]));
            setTurnipColor(buyPrice, WednesdayAM, turnipSell3AM);

            turnipSell3PM.Clear();
            turnipSell3PM.Text = String.Format("{0}", turnipPrices[5]);
            int WednesdayPM = int.Parse(String.Format("{0}", turnipPrices[5]));
            setTurnipColor(buyPrice, WednesdayPM, turnipSell3PM);

            turnipSell4AM.Clear();
            turnipSell4AM.Text = String.Format("{0}", turnipPrices[6]);
            int ThursdayAM = int.Parse(String.Format("{0}", turnipPrices[6]));
            setTurnipColor(buyPrice, ThursdayAM, turnipSell4AM);

            turnipSell4PM.Clear();
            turnipSell4PM.Text = String.Format("{0}", turnipPrices[7]);
            int ThursdayPM = int.Parse(String.Format("{0}", turnipPrices[7]));
            setTurnipColor(buyPrice, ThursdayPM, turnipSell4PM);

            turnipSell5AM.Clear();
            turnipSell5AM.Text = String.Format("{0}", turnipPrices[8]);
            int FridayAM = int.Parse(String.Format("{0}", turnipPrices[8]));
            setTurnipColor(buyPrice, FridayAM, turnipSell5AM);

            turnipSell5PM.Clear();
            turnipSell5PM.Text = String.Format("{0}", turnipPrices[9]);
            int FridayPM = int.Parse(String.Format("{0}", turnipPrices[9]));
            setTurnipColor(buyPrice, FridayPM, turnipSell5PM);

            turnipSell6AM.Clear();
            turnipSell6AM.Text = String.Format("{0}", turnipPrices[10]);
            int SaturdayAM = int.Parse(String.Format("{0}", turnipPrices[10]));
            setTurnipColor(buyPrice, SaturdayAM, turnipSell6AM);

            turnipSell6PM.Clear();
            turnipSell6PM.Text = String.Format("{0}", turnipPrices[11]);
            int SaturdayPM = int.Parse(String.Format("{0}", turnipPrices[11]));
            setTurnipColor(buyPrice, SaturdayPM, turnipSell6PM);

            int[] price = { MondayAM, MondayPM, TuesdayAM, TuesdayPM, WednesdayAM, WednesdayPM, ThursdayAM, ThursdayPM, FridayAM, FridayPM, SaturdayAM, SaturdayPM };
            int highest = findHighest(price);
            setStar(highest, MondayAM, MondayPM, TuesdayAM, TuesdayPM, WednesdayAM, WednesdayPM, ThursdayAM, ThursdayPM, FridayAM, FridayPM, SaturdayAM, SaturdayPM);
        }

        private void setTurnipColor(int buyPrice, int comparePrice, RichTextBox target)
        {
            target.SelectionAlignment = HorizontalAlignment.Center;

            if (comparePrice > buyPrice)
            {
                target.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            }
            else if (comparePrice < buyPrice)
            {
                target.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            }
        }

        private int findHighest(int[] price)
        {
            int highest = -1;
            for (int i = 0; i < price.Length; i++)
            {
                if (price[i] > highest)
                {
                    highest = price[i];
                }
            }
            return highest;
        }

        private void setStar(int highest, int MondayAM, int MondayPM, int TuesdayAM, int TuesdayPM, int WednesdayAM, int WednesdayPM, int ThursdayAM, int ThursdayPM, int FridayAM, int FridayPM, int SaturdayAM, int SaturdayPM)
        {
            if (MondayAM >= highest) { mondayAMStar.Visible = true; } else { mondayAMStar.Visible = false; };
            if (MondayPM >= highest) { mondayPMStar.Visible = true; } else { mondayPMStar.Visible = false; };

            if (TuesdayAM >= highest) { tuesdayAMStar.Visible = true; } else { tuesdayAMStar.Visible = false; };
            if (TuesdayPM >= highest) { tuesdayPMStar.Visible = true; } else { tuesdayPMStar.Visible = false; };

            if (WednesdayAM >= highest) { wednesdayAMStar.Visible = true; } else { wednesdayAMStar.Visible = false; };
            if (WednesdayPM >= highest) { wednesdayPMStar.Visible = true; } else { wednesdayPMStar.Visible = false; };

            if (ThursdayAM >= highest) { thursdayAMStar.Visible = true; } else { thursdayAMStar.Visible = false; };
            if (ThursdayPM >= highest) { thursdayPMStar.Visible = true; } else { thursdayPMStar.Visible = false; };

            if (FridayAM >= highest) { fridayAMStar.Visible = true; } else { fridayAMStar.Visible = false; };
            if (FridayPM >= highest) { fridayPMStar.Visible = true; } else { fridayPMStar.Visible = false; };

            if (SaturdayAM >= highest) { saturdayAMStar.Visible = true; } else { saturdayAMStar.Visible = false; };
            if (SaturdayPM >= highest) { saturdayPMStar.Visible = true; } else { saturdayPMStar.Visible = false; };
        }

        private void loadReaction()
        {
            byte[] reactionBank = Utilities.getReaction(s, bot);
            //Debug.Print(Encoding.ASCII.GetString(reactionBank));

            byte[] reaction1 = new byte[1];
            byte[] reaction2 = new byte[1];
            byte[] reaction3 = new byte[1];
            byte[] reaction4 = new byte[1];
            byte[] reaction5 = new byte[1];
            byte[] reaction6 = new byte[1];
            byte[] reaction7 = new byte[1];
            byte[] reaction8 = new byte[1];

            Buffer.BlockCopy(reactionBank, 0, reaction1, 0x0, 0x1);
            Buffer.BlockCopy(reactionBank, 1, reaction2, 0x0, 0x1);
            Buffer.BlockCopy(reactionBank, 2, reaction3, 0x0, 0x1);
            Buffer.BlockCopy(reactionBank, 3, reaction4, 0x0, 0x1);
            Buffer.BlockCopy(reactionBank, 4, reaction5, 0x0, 0x1);
            Buffer.BlockCopy(reactionBank, 5, reaction6, 0x0, 0x1);
            Buffer.BlockCopy(reactionBank, 6, reaction7, 0x0, 0x1);
            Buffer.BlockCopy(reactionBank, 7, reaction8, 0x0, 0x1);

            setReactionBox(Utilities.ByteToHexString(reaction1), reactionSlot1);
            setReactionBox(Utilities.ByteToHexString(reaction2), reactionSlot2);
            setReactionBox(Utilities.ByteToHexString(reaction3), reactionSlot3);
            setReactionBox(Utilities.ByteToHexString(reaction4), reactionSlot4);
            setReactionBox(Utilities.ByteToHexString(reaction5), reactionSlot5);
            setReactionBox(Utilities.ByteToHexString(reaction6), reactionSlot6);
            setReactionBox(Utilities.ByteToHexString(reaction7), reactionSlot7);
            setReactionBox(Utilities.ByteToHexString(reaction8), reactionSlot8);
            //Debug.Print(Encoding.ASCII.GetString(reaction1) + " | " + Encoding.ASCII.GetString(reaction2) + " | " + Encoding.ASCII.GetString(reaction3) + " | " + Encoding.ASCII.GetString(reaction4));
            //Debug.Print(Encoding.ASCII.GetString(reaction5) + " | " + Encoding.ASCII.GetString(reaction6) + " | " + Encoding.ASCII.GetString(reaction7) + " | " + Encoding.ASCII.GetString(reaction8));
        }

        private void setReactionBox(string reaction, ComboBox box)
        {
            if (reaction == "00")
            {
                return;
            }
            string hexValue = reaction;
            int decValue = Convert.ToInt32(hexValue, 16) - 1;
            box.SelectedIndex = decValue;
        }

        private void setReactionBtn_Click(object sender, EventArgs e)
        {
            System.Media.SystemSounds.Asterisk.Play();
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to change your reaction wheel?\n[Warning] Your previous reaction wheel will be overwritten!", "Change Reaction Wheel", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string reaction1 = (Utilities.precedingZeros((reactionSlot1.SelectedIndex + 1).ToString("X"), 2) + Utilities.precedingZeros((reactionSlot2.SelectedIndex + 1).ToString("X"), 2) + Utilities.precedingZeros((reactionSlot3.SelectedIndex + 1).ToString("X"), 2) + Utilities.precedingZeros((reactionSlot4.SelectedIndex + 1).ToString("X"), 2));
                string reaction2 = (Utilities.precedingZeros((reactionSlot5.SelectedIndex + 1).ToString("X"), 2) + Utilities.precedingZeros((reactionSlot6.SelectedIndex + 1).ToString("X"), 2) + Utilities.precedingZeros((reactionSlot7.SelectedIndex + 1).ToString("X"), 2) + Utilities.precedingZeros((reactionSlot8.SelectedIndex + 1).ToString("X"), 2));
                Utilities.setReaction(s, bot, reaction1, reaction2);
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void speedEnableBtn_Click(object sender, EventArgs e)
        {
            speedEnableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            speedDisableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.wSpeedAddress.ToString("X"), "1E221001");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void speedDisableBtn_Click(object sender, EventArgs e)
        {
            speedDisableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            speedEnableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.wSpeedAddress.ToString("X"), "BD4FEE61");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void disableCollisionBtn_Click(object sender, EventArgs e)
        {
            disableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            enableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.CollisionAddress.ToString("X"), "12800014");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void enableCollisionBtn_Click(object sender, EventArgs e)
        {
            enableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            disableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.CollisionAddress.ToString("X"), "B953F814");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void freezeTimeBtn_Click(object sender, EventArgs e)
        {
            freezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            unfreezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.freezeTimeAddress.ToString("X"), "D503201F");
            readtime();
            timePanel.Visible = true;

            System.Media.SystemSounds.Asterisk.Play();
        }

        private void unfreezeTimeBtn_Click(object sender, EventArgs e)
        {
            unfreezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            freezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.freezeTimeAddress.ToString("X"), "F9203260");
            timePanel.Visible = false;
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx2_Click(object sender, EventArgs e)
        {
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "40000000");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx0_1_Click(object sender, EventArgs e)
        {
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "3DCCCCCD");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx50_Click(object sender, EventArgs e)
        {
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "42480000");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx1_Click(object sender, EventArgs e)
        {
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            Utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "3F800000");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void readtime()
        {
            byte[] b = Utilities.peekAddress(s, bot, Utilities.readTimeAddress, 6);
            string time = Utilities.ByteToHexString(b);
            Debug.Print(time);

            yearTextbox.Clear();
            yearTextbox.SelectionAlignment = HorizontalAlignment.Center;
            yearTextbox.Text = Convert.ToInt32(Utilities.flip(time.Substring(0, 4)), 16).ToString();

            monthTextbox.Clear();
            monthTextbox.SelectionAlignment = HorizontalAlignment.Center;
            monthTextbox.Text = Convert.ToInt32((time.Substring(4, 2)), 16).ToString();

            dayTextbox.Clear();
            dayTextbox.SelectionAlignment = HorizontalAlignment.Center;
            dayTextbox.Text = Convert.ToInt32((time.Substring(6, 2)), 16).ToString();

            hourTextbox.Clear();
            hourTextbox.SelectionAlignment = HorizontalAlignment.Center;
            hourTextbox.Text = Convert.ToInt32((time.Substring(8, 2)), 16).ToString();

            minTextbox.Clear();
            minTextbox.SelectionAlignment = HorizontalAlignment.Center;
            minTextbox.Text = Convert.ToInt32((time.Substring(10, 2)), 16).ToString();

        }

        private void settimeBtn_Click(object sender, EventArgs e)
        {
            int decYear = int.Parse(yearTextbox.Text);
            if (decYear > 2060)
            {
                decYear = 2060;
            }
            else if (decYear < 1970)
            {
                decYear = 1970;
            }
            yearTextbox.Text = decYear.ToString();
            string hexYear = decYear.ToString("X");

            int decMonth = int.Parse(monthTextbox.Text);
            if (decMonth > 12)
            {
                decMonth = 12;
            }
            else if (decMonth < 0)
            {
                decMonth = 1;
            }
            monthTextbox.Text = decMonth.ToString();
            string hexMonth = decMonth.ToString("X");

            int decDay = int.Parse(dayTextbox.Text);
            if (decDay > 31)
            {
                decDay = 31;
            }
            else if (decDay < 0)
            {
                decDay = 1;
            }
            dayTextbox.Text = decDay.ToString();
            string hexDay = decDay.ToString("X");

            int decHour = int.Parse(hourTextbox.Text);
            if (decHour > 23)
            {
                decHour = 23;
            }
            else if (decHour < 0)
            {
                decHour = 0;
            }
            hourTextbox.Text = decHour.ToString();
            string hexHour = decHour.ToString("X");


            int decMin = int.Parse(minTextbox.Text);
            if (decMin > 59)
            {
                decMin = 59;
            }
            else if (decMin < 0)
            {
                decMin = 0;
            }
            minTextbox.Text = decMin.ToString();
            string hexMin = decMin.ToString("X");

            Utilities.pokeAddress(s, bot, "0x" + Utilities.readTimeAddress.ToString("X"), Utilities.flip(Utilities.precedingZeros(hexYear, 4)));

            Utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x2).ToString("X"), Utilities.precedingZeros(hexMonth, 2) + Utilities.precedingZeros(hexDay, 2) + Utilities.precedingZeros(hexHour, 2) + Utilities.precedingZeros(hexMin, 2));

            System.Media.SystemSounds.Asterisk.Play();
        }

        private void minus1HourBtn_Click(object sender, EventArgs e)
        {
            int decHour = int.Parse(hourTextbox.Text) - 1;
            if (decHour < 0)
            {
                decHour = 23;
                string hexHour = decHour.ToString("X");

                int decDay = int.Parse(dayTextbox.Text) - 1;
                string hexDay = decDay.ToString("X");

                Utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x3).ToString("X"), Utilities.precedingZeros(hexDay, 2) + Utilities.precedingZeros(hexHour, 2));
            }
            else
            {
                string hexHour = decHour.ToString("X");
                Utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x4).ToString("X"), Utilities.precedingZeros(hexHour, 2));
            }
            readtime();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void add1HourBtn_Click(object sender, EventArgs e)
        {
            int decHour = int.Parse(hourTextbox.Text) + 1;
            if (decHour >= 24)
            {
                decHour = 0;
                string hexHour = decHour.ToString("X");

                int decDay = int.Parse(dayTextbox.Text) + 1;
                string hexDay = decDay.ToString("X");

                Utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x3).ToString("X"), Utilities.precedingZeros(hexDay, 2) + Utilities.precedingZeros(hexHour, 2));
            }
            else
            {
                string hexHour = decHour.ToString("X");
                Utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x4).ToString("X"), Utilities.precedingZeros(hexHour, 2));
            }
            readtime();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void USBconnectBtn_Click(object sender, EventArgs e)
        {

            if (USBconnectBtn.Tag.ToString() == "connect")
            {
                bot = new USBBot();
                if (bot.Connect())
                {

                    if (UpdateInventory())
                        return;

                    UpdateTownID();
                    //InitTimer();
                    setEatBtn();
                    UpdateTurnipPrices();
                    loadReaction();
                    readWeatherSeed();
                    this.connectBtn.Visible = false;
                    this.refreshBtn.Visible = true;
                    this.playerSelectionPanel.Visible = true;
                    //this.autoRefreshCheckBox.Visible = true;
                    this.autoRefreshCheckBox.Checked = false;
                    this.saveBtn.Visible = true;
                    this.loadBtn.Visible = true;
                    this.otherBtn.Visible = true;
                    this.critterBtn.Visible = true;
                    this.villagerBtn.Visible = true;
                    this.wrapSetting.SelectedIndex = 0;
                    //this.selectedItem.setHide(true);
                    this.ipBox.Visible = false;
                    this.pictureBox1.Visible = false;
                    this.pokeMainCheatPanel.Visible = false;
                    this.Text += "  |  [Connected via USB]";

                    currentGridView = insectGridView;

                    LoadGridView(InsectAppearParam, insectGridView, ref insectRate, Utilities.InsectDataSize, Utilities.InsectNumRecords);
                    LoadGridView(FishRiverAppearParam, riverFishGridView, ref riverFishRate, Utilities.FishDataSize, Utilities.FishRiverNumRecords, 1);
                    LoadGridView(FishSeaAppearParam, seaFishGridView, ref seaFishRate, Utilities.FishDataSize, Utilities.FishSeaNumRecords, 1);
                    LoadGridView(CreatureSeaAppearParam, seaCreatureGridView, ref seaCreatureRate, Utilities.SeaCreatureDataSize, Utilities.SeaCreatureNumRecords, 1);

                    USBconnectBtn.Text = "Disconnect";
                    USBconnectBtn.Tag = "Disconnect";

                    offline = false;
                }
                else
                {
                    bot = null;
                }
            }
            else
            {
                bot.Disconnect();

                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    btn.reset();
                }

                this.connectBtn.Visible = true;
                this.connectBtn.Enabled = true;
                this.refreshBtn.Visible = false;
                this.playerSelectionPanel.Visible = false;
                this.autoRefreshCheckBox.Visible = false;

                //this.saveBtn.Visible = false;
                //this.loadBtn.Visible = false;
                inventoryBtn_Click(sender, e);
                this.otherBtn.Visible = false;
                this.critterBtn.Visible = false;
                this.villagerBtn.Visible = false;
                this.ipBox.Visible = true;
                this.pictureBox1.Visible = true;
                cleanVillagerPage();

                this.USBconnectBtn.Text = "USB";
                this.USBconnectBtn.Tag = "connect";
                this.Text = version;
            }
        }

        private void egg_Click(object sender, EventArgs e)
        {
            if (egg.Tag.ToString() == "Play")
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                mciSendString("open \"" + path + villagerPath + "Io.nhv" + "\" type mpegvideo alias MediaFile", null, 0, IntPtr.Zero);
                mciSendString("play MediaFile repeat", null, 0, IntPtr.Zero);
                egg.Tag = "Stop";
            }
            else
            {
                mciSendString("close MediaFile", null, 0, IntPtr.Zero);
                egg.Tag = "Play";
            }
        }
    }
}
