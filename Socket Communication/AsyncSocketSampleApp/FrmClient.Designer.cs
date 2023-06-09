﻿namespace AsyncSocketSampleApp
{
    partial class FrmClient
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다.
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마십시오.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tb_ip = new System.Windows.Forms.TextBox();
            this.tb_port = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btn_BHS = new System.Windows.Forms.Button();
            this.btn_HWS = new System.Windows.Forms.Button();
            this.btn_BGW = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(12, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(418, 76);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(12, 94);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(418, 76);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(12, 176);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(418, 76);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(12, 431);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(417, 165);
            this.txtMessage.TabIndex = 3;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(80, 358);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(350, 21);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "carinfo,";
            // 
            // tb_ip
            // 
            this.tb_ip.Location = new System.Drawing.Point(80, 258);
            this.tb_ip.Name = "tb_ip";
            this.tb_ip.Size = new System.Drawing.Size(351, 21);
            this.tb_ip.TabIndex = 0;
            this.tb_ip.Text = "127.0.0.1";
            // 
            // tb_port
            // 
            this.tb_port.Location = new System.Drawing.Point(80, 285);
            this.tb_port.Name = "tb_port";
            this.tb_port.Size = new System.Drawing.Size(349, 21);
            this.tb_port.TabIndex = 1;
            this.tb_port.Text = "9988";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(447, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(311, 76);
            this.button1.TabIndex = 0;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(447, 94);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(311, 76);
            this.button2.TabIndex = 0;
            this.button2.Text = "STOP";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // btn_BHS
            // 
            this.btn_BHS.Location = new System.Drawing.Point(447, 303);
            this.btn_BHS.Name = "btn_BHS";
            this.btn_BHS.Size = new System.Drawing.Size(311, 76);
            this.btn_BHS.TabIndex = 0;
            this.btn_BHS.Text = "BHS,";
            this.btn_BHS.UseVisualStyleBackColor = true;
            this.btn_BHS.Click += new System.EventHandler(this.btn_BHS_Click);
            // 
            // btn_HWS
            // 
            this.btn_HWS.Location = new System.Drawing.Point(447, 385);
            this.btn_HWS.Name = "btn_HWS";
            this.btn_HWS.Size = new System.Drawing.Size(311, 76);
            this.btn_HWS.TabIndex = 0;
            this.btn_HWS.Text = "HWS,";
            this.btn_HWS.UseVisualStyleBackColor = true;
            this.btn_HWS.Click += new System.EventHandler(this.btn_HWS_Click);
            // 
            // btn_BGW
            // 
            this.btn_BGW.Location = new System.Drawing.Point(447, 546);
            this.btn_BGW.Name = "btn_BGW";
            this.btn_BGW.Size = new System.Drawing.Size(311, 76);
            this.btn_BGW.TabIndex = 0;
            this.btn_BGW.Text = "BGW,";
            this.btn_BGW.UseVisualStyleBackColor = true;
            this.btn_BGW.Click += new System.EventHandler(this.btn_BGW_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(447, 464);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(311, 76);
            this.button3.TabIndex = 0;
            this.button3.Text = "HWS,";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.btn_CWS_Click);
            // 
            // FrmClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(770, 634);
            this.Controls.Add(this.tb_port);
            this.Controls.Add(this.tb_ip);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.btn_HWS);
            this.Controls.Add(this.btn_BGW);
            this.Controls.Add(this.btn_BHS);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnConnect);
            this.Name = "FrmClient";
            this.Text = "클라이언트";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox tb_ip;
        private System.Windows.Forms.TextBox tb_port;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btn_BHS;
        private System.Windows.Forms.Button btn_HWS;
        private System.Windows.Forms.Button btn_BGW;
        private System.Windows.Forms.Button button3;
    }
}

