using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Security.Cryptography;

namespace iLocker
{
    public partial class iLockerForm : Form
    {
        Encoding defaultEncoding = Encoding.UTF8;
        BackgroundWorker encryptWorker, decryptWorker;

        public iLockerForm()
        {
            InitializeComponent();
            encryptWorker = new BackgroundWorker();
            encryptWorker.DoWork += new DoWorkEventHandler(Encrypt);
            encryptWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(EncryptionCompleted);


            decryptWorker = new BackgroundWorker();
            decryptWorker.DoWork += new DoWorkEventHandler(Decrypt);
            decryptWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DecryptionCompleted);
        }
        private void Encrypt(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                string sInfo = "Encrypted";
                foreach (ListViewItem item in listView.Items)
                {
                    string sFilePath = item.ToolTipText;
                    string sKey = txtPassword.Text.Trim();

                    if (File.Exists(sFilePath) == false)
                        continue;

                    if (item.SubItems.Count > 1 && item.SubItems[1].Text == sInfo)
                        continue;

                    RijndaelManaged AES = new RijndaelManaged();
                    AES.Key = MD5(SHA(sKey));
                    AES.IV = MD5(SHA(MD5(sKey)));

                    FileStream fsCrypt = new FileStream(sFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);

                    CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);

                    FileStream fsIn = new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    int data;
                    while ((data = fsIn.ReadByte()) != -1)
                        cs.WriteByte((byte)data);


                    fsIn.Close();
                    cs.Close();
                    fsCrypt.Close();

                    if (item.SubItems.Count <= 1)
                        item.SubItems.Add(sInfo);
                    else
                        item.SubItems[1].Text = sInfo;

                }
            
            }));

          
        }

        private void Decrypt(object sender, DoWorkEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate()
            {
                string sInfo = "Decrypted";
                foreach (ListViewItem item in listView.Items)
                {
                    string sFilePath = item.ToolTipText;
                    string sKey = txtPassword.Text.Trim();

                    if (File.Exists(sFilePath) == false)
                        continue;

                    if (item.SubItems.Count > 1 && item.SubItems[1].Text == sInfo)
                        continue;

                    RijndaelManaged AES = new RijndaelManaged();
                    AES.Key = MD5(SHA(sKey));
                    AES.IV = MD5(SHA(MD5(sKey)));

                    FileStream fsCrypt = new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read);

                    FileStream fsOut = new FileStream(sFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);

                    int data;
                    while ((data = cs.ReadByte()) != -1)
                        fsOut.WriteByte((byte)data);

                    fsOut.Close();
                    cs.Close();
                    fsCrypt.Close();

                    if (item.SubItems.Count <= 1)
                        item.SubItems.Add(sInfo);
                    else
                        item.SubItems[1].Text = sInfo;

                }
            }));
        }

        private byte[] MD5(string sPlainText)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(defaultEncoding.GetBytes(sPlainText));
        }

        private byte[] MD5(byte[] sPlainText)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(sPlainText);
        }
        private byte[] SHA(string sPlainText)
        {
            SHA256Managed sha = new SHA256Managed();
            return sha.ComputeHash(defaultEncoding.GetBytes(sPlainText));
        }

        private byte[] SHA(byte[] sPlainText)
        {
            SHA256Managed sha = new SHA256Managed();
            return sha.ComputeHash(sPlainText);
        }


        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            if(txtPassword.Text.Trim() == "")
            {
                MessageBox.Show("Please set password first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblShowInfo.Text = "Encryption Running...";
            mainPanel.Enabled = false;


            encryptWorker.RunWorkerAsync();
        }

        private void btnDecrypt_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text.Trim() == "")
            {
                MessageBox.Show("Please set password first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            lblShowInfo.Text = "Decryption Running...";
            mainPanel.Enabled = false;

            decryptWorker.RunWorkerAsync();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            DialogResult re = openFileDialog.ShowDialog();
            if (re == System.Windows.Forms.DialogResult.OK)
            {
                foreach (string file in openFileDialog.FileNames)
                {
                    if (File.Exists(file) && listView.Items.ContainsKey(file) == false)
                    {
                        listView.Items.Add(file, Path.GetFileName(file), file).ToolTipText = file;
                    }                    
                }
            }
        }

        private void EncryptionCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblShowInfo.Text = "";
            mainPanel.Enabled = true;
        }

        private void DecryptionCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblShowInfo.Text = "";
            mainPanel.Enabled = true;
        }

        private void iLockerForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string file in files)
            {
                if (File.Exists(file) && listView.Items.ContainsKey(file) == false)
                {
                    listView.Items.Add(file, Path.GetFileName(file), file).ToolTipText = file;
                }
            }
        }

        private void iLockerForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void listView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            {
                if (listView.SelectedItems.Count > 0)
                {
                    int lastSelectedItemIdx = 0;
                    foreach (ListViewItem item in listView.SelectedItems)
                    {
                        lastSelectedItemIdx = item.Index;
                        listView.Items.Remove(item);
                    }
                    if (listView.Items.Count > 0 && ( lastSelectedItemIdx < 0 || lastSelectedItemIdx >= listView.Items.Count))
                    {
                        lastSelectedItemIdx = 0;
                        listView.Items[lastSelectedItemIdx].Selected = true;
                    }
                }
            }
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            if( e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem item in listView.Items)
                    item.Selected = true;
            }
        }

        private void lblCopyright_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("iLocker © 2015\nYuv.me", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
