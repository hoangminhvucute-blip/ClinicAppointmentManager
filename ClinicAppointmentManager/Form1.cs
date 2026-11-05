
#nullable disable

using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ClinicAppointmentManager
{
    public partial class Form1 : Form
    {
        private BindingList<Appointment> appointments =
            new BindingList<Appointment>();

        private BindingList<Appointment> displayList =
            new BindingList<Appointment>();

        private BindingSource bindingSource = new BindingSource();
        private ErrorProvider errorProvider = new ErrorProvider();

        private Color primaryColor = Color.FromArgb(35, 99, 180);
        private Color darkColor = Color.FromArgb(25, 55, 95);
        private Color backgroundColor = Color.FromArgb(243, 247, 252);

        private TextBox txtId = new TextBox();
        private TextBox txtName = new TextBox();
        private TextBox txtPhone = new TextBox();
        private TextBox txtReason = new TextBox();
        private TextBox txtSearch = new TextBox();

        private DateTimePicker dtpAppointment = new DateTimePicker();
        private DateTimePicker dtpFilterDate = new DateTimePicker();

        private ComboBox cboDoctor = new ComboBox();
        private ComboBox cboStatus = new ComboBox();
        private ComboBox cboFilterStatus = new ComboBox();

        private CheckBox chkFilterDate = new CheckBox();

        private DataGridView dgvAppointments = new DataGridView();

        private ToolStripStatusLabel lblCount =
            new ToolStripStatusLabel("0");

        private Label lblTotal = new Label();
        private Label lblWaiting = new Label();
        private Label lblInProgress = new Label();
        private Label lblCompleted = new Label();

        private Button btnAdd = new Button();
        private Button btnUpdate = new Button();
        private Button btnDelete = new Button();
        private Button btnClear = new Button();

        public Form1()
        {
            InitializeComponent();

            this.Text = "Clinic Appointment Manager";
            this.Size = new Size(1350, 900);
            this.MinimumSize = new Size(1150, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = backgroundColor;
            this.Font = new Font("Segoe UI", 9F);

            errorProvider.ContainerControl = this;

            BuildInterface();
            ConfigureControls();
            ConfigureGrid();

            bindingSource.DataSource = displayList;
            dgvAppointments.DataSource = bindingSource;

            ApplyFilter();
        }

        // ================= GIAO DIỆN CHÍNH =================

        private void BuildInterface()
        {
            // MENU
            MenuStrip menu = new MenuStrip();
            menu.BackColor = Color.White;
            menu.Font = new Font("Segoe UI", 9F);

            ToolStripMenuItem menuFile =
                new ToolStripMenuItem("Tệp");

            ToolStripMenuItem menuExport =
                new ToolStripMenuItem("Xuất danh sách CSV");

            menuExport.Click += MenuExport_Click;

            menuFile.DropDownItems.Add(menuExport);
            menu.Items.Add(menuFile);

            this.MainMenuStrip = menu;

            // STATUS STRIP
            StatusStrip status = new StatusStrip();
            status.BackColor = Color.White;

            status.Items.Add(
                new ToolStripStatusLabel(
                    "Tổng số lịch hẹn sau khi lọc:"));

            status.Items.Add(lblCount);

            // TABLELAYOUTPANEL CHÍNH
            TableLayoutPanel main = new TableLayoutPanel();

            main.Dock = DockStyle.Fill;
            main.BackColor = backgroundColor;
            main.Padding = new Padding(18, 12, 18, 12);
            main.ColumnCount = 1;
            main.RowCount = 5;

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 75));

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 100));

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 300));

            main.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 145));

            main.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            // HEADER
            Panel header = new Panel();
            header.Dock = DockStyle.Fill;
            header.BackColor = darkColor;

            Label lblTitle = new Label();
            lblTitle.Text = "CLINIC APPOINTMENT MANAGER";
            lblTitle.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 10);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "HỆ THỐNG QUẢN LÝ LỊCH HẸN PHÒNG KHÁM";
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = Color.FromArgb(205, 220, 240);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 43);

            header.Controls.Add(lblTitle);
            header.Controls.Add(lblSubtitle);

            main.Controls.Add(header, 0, 0);

            // THỐNG KÊ
            TableLayoutPanel stats = new TableLayoutPanel();

            stats.Dock = DockStyle.Fill;
            stats.ColumnCount = 4;
            stats.RowCount = 1;
            stats.Padding = new Padding(0, 8, 0, 5);

            for (int i = 0; i < 4; i++)
            {
                stats.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 25));
            }

            stats.Controls.Add(
                CreateStatCard("TỔNG LỊCH HẸN",
                Color.FromArgb(35, 99, 180), out lblTotal), 0, 0);

            stats.Controls.Add(
                CreateStatCard("CHỜ KHÁM",
                Color.FromArgb(230, 160, 40), out lblWaiting), 1, 0);

            stats.Controls.Add(
                CreateStatCard("ĐANG KHÁM",
                Color.FromArgb(35, 145, 190), out lblInProgress), 2, 0);

            stats.Controls.Add(
                CreateStatCard("HOÀN TẤT",
                Color.FromArgb(35, 155, 105), out lblCompleted), 3, 0);

            main.Controls.Add(stats, 0, 1);

            // KHU VỰC NHẬP THÔNG TIN
            GroupBox groupInput = CreateGroupBox("THÔNG TIN LỊCH HẸN");

            TableLayoutPanel input = new TableLayoutPanel();

            input.Dock = DockStyle.Fill;
            input.Padding = new Padding(12, 10, 12, 8);
            input.ColumnCount = 4;
            input.RowCount = 5;

            input.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 145));

            input.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            input.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 145));

            input.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            input.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            input.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            input.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
            input.RowStyles.Add(new RowStyle(SizeType.Percent, 22));
            input.RowStyles.Add(new RowStyle(SizeType.Percent, 24));

            AddInput(input, "Mã lịch hẹn:", txtId, 0, 0);
            AddInput(input, "Họ tên bệnh nhân:", txtName, 2, 0);

            AddInput(input, "Số điện thoại:", txtPhone, 0, 1);
            AddInput(input, "Ngày giờ hẹn:", dtpAppointment, 2, 1);

            AddInput(input, "Bác sĩ:", cboDoctor, 0, 2);
            AddInput(input, "Trạng thái:", cboStatus, 2, 2);

            AddInput(input, "Lý do khám:", txtReason, 0, 3);
            input.SetColumnSpan(txtReason, 3);

            // NÚT CHỨC NĂNG
            FlowLayoutPanel buttons = new FlowLayoutPanel();

            buttons.Dock = DockStyle.Fill;
            buttons.WrapContents = false;
            buttons.FlowDirection = FlowDirection.LeftToRight;
            buttons.Padding = new Padding(0, 8, 0, 0);

            StyleButton(btnAdd, "＋  Thêm mới",
                Color.FromArgb(35, 155, 105));

            StyleButton(btnUpdate, "✎  Cập nhật",
                primaryColor);

            StyleButton(btnDelete, "✕  Xóa",
                Color.FromArgb(210, 70, 70));

            StyleButton(btnClear, "↻  Làm mới",
                Color.FromArgb(105, 120, 140));

            buttons.Controls.Add(btnAdd);
            buttons.Controls.Add(btnUpdate);
            buttons.Controls.Add(btnDelete);
            buttons.Controls.Add(btnClear);

            input.Controls.Add(buttons, 0, 4);
            input.SetColumnSpan(buttons, 4);

            groupInput.Controls.Add(input);
            main.Controls.Add(groupInput, 0, 2);

            // TÌM KIẾM VÀ LỌC
            GroupBox groupFilter =
                CreateGroupBox("TÌM KIẾM VÀ LỌC LỊCH HẸN");

            TableLayoutPanel filter = new TableLayoutPanel();

            filter.Dock = DockStyle.Fill;
            filter.Padding = new Padding(12, 8, 12, 5);
            filter.ColumnCount = 6;
            filter.RowCount = 1;

            filter.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 85));

            filter.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 35));

            filter.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 95));

            filter.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25));

            filter.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 145));

            filter.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 40));

            Label lblSearch = new Label();
            lblSearch.Text = "Tìm kiếm:";
            lblSearch.Dock = DockStyle.Fill;
            lblSearch.TextAlign = ContentAlignment.MiddleLeft;

            Label lblDate = new Label();
            lblDate.Text = "Lọc ngày:";
            lblDate.Dock = DockStyle.Fill;
            lblDate.TextAlign = ContentAlignment.MiddleLeft;

            Label lblStatus = new Label();
            lblStatus.Text = "Trạng thái:";
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            txtSearch.Dock = DockStyle.Fill;
            txtSearch.Margin = new Padding(3, 5, 10, 5);

            chkFilterDate.Text = "Bật";
            chkFilterDate.Dock = DockStyle.Fill;
            chkFilterDate.TextAlign = ContentAlignment.MiddleCenter;

            dtpFilterDate.Dock = DockStyle.Fill;
            dtpFilterDate.Margin = new Padding(3, 5, 10, 5);

            cboFilterStatus.Dock = DockStyle.Fill;
            cboFilterStatus.Margin = new Padding(3, 5, 3, 5);

            filter.Controls.Add(lblSearch, 0, 0);
            filter.Controls.Add(txtSearch, 1, 0);
            filter.Controls.Add(lblDate, 2, 0);
            filter.Controls.Add(chkFilterDate, 3, 0);
            filter.Controls.Add(lblStatus, 4, 0);
            filter.Controls.Add(cboFilterStatus, 5, 0);

            // Thêm bộ chọn ngày ở hàng phụ
            TableLayoutPanel filterContainer = new TableLayoutPanel();
            filterContainer.Dock = DockStyle.Fill;
            filterContainer.RowCount = 2;
            filterContainer.ColumnCount = 1;

            filterContainer.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            filterContainer.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            filterContainer.Controls.Add(filter, 0, 0);

            FlowLayoutPanel datePanel = new FlowLayoutPanel();
            datePanel.Dock = DockStyle.Fill;
            datePanel.FlowDirection = FlowDirection.LeftToRight;
            datePanel.Padding = new Padding(12, 0, 0, 0);

            Label lblChooseDate = new Label();
            lblChooseDate.Text = "Ngày hẹn:";
            lblChooseDate.AutoSize = true;
            lblChooseDate.Margin = new Padding(0, 7, 8, 0);

            dtpFilterDate.Width = 180;
            dtpFilterDate.Enabled = false;

            datePanel.Controls.Add(lblChooseDate);
            datePanel.Controls.Add(dtpFilterDate);

            filterContainer.Controls.Add(datePanel, 0, 1);

            groupFilter.Controls.Add(filterContainer);
            main.Controls.Add(groupFilter, 0, 3);

            // DANH SÁCH
            GroupBox groupGrid =
                CreateGroupBox("DANH SÁCH LỊCH HẸN");

            groupGrid.Padding = new Padding(10);

            dgvAppointments.Dock = DockStyle.Fill;
            dgvAppointments.Margin = new Padding(5);

            groupGrid.Controls.Add(dgvAppointments);

            main.Controls.Add(groupGrid, 0, 4);

            // ĐƯA VÀO FORM
            this.Controls.Add(main);
            this.Controls.Add(status);
            this.Controls.Add(menu);

            menu.BringToFront();
            status.BringToFront();
        }

        // ================= THẺ THỐNG KÊ =================

        private Panel CreateStatCard(
            string title, Color accent, out Label valueLabel)
        {
            Panel card = new Panel();

            card.Dock = DockStyle.Fill;
            card.BackColor = Color.White;
            card.Margin = new Padding(5, 3, 5, 3);

            Panel line = new Panel();
            line.Width = 5;
            line.Dock = DockStyle.Left;
            line.BackColor = accent;

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.ForeColor = Color.Gray;
            lblTitle.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(17, 10);

            valueLabel = new Label();
            valueLabel.Text = "0";
            valueLabel.Font = new Font("Segoe UI", 21F, FontStyle.Bold);
            valueLabel.ForeColor = accent;
            valueLabel.AutoSize = true;
            valueLabel.Location = new Point(16, 30);

            card.Controls.Add(lblTitle);
            card.Controls.Add(valueLabel);
            card.Controls.Add(line);

            return card;
        }

        // ================= GROUPBOX =================

        private GroupBox CreateGroupBox(string title)
        {
            GroupBox group = new GroupBox();

            group.Text = title;
            group.Dock = DockStyle.Fill;
            group.BackColor = Color.White;
            group.ForeColor = darkColor;
            group.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            group.Padding = new Padding(10);

            return group;
        }

        // ================= TẠO INPUT =================

        private void AddInput(
            TableLayoutPanel panel,
            string title,
            Control control,
            int column,
            int row)
        {
            Label label = new Label();

            label.Text = title;
            label.Dock = DockStyle.Fill;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.ForeColor = darkColor;
            label.Font = new Font("Segoe UI", 9F);

            control.Dock = DockStyle.Fill;
            control.Margin = new Padding(3, 5, 12, 5);
            control.Font = new Font("Segoe UI", 9F);

            panel.Controls.Add(label, column, row);
            panel.Controls.Add(control, column + 1, row);
        }

        // ================= STYLE BUTTON =================

        private void StyleButton(
            Button button, string title, Color color)
        {
            button.Text = title;
            button.Width = 125;
            button.Height = 36;
            button.Margin = new Padding(3, 2, 8, 2);

            button.BackColor = color;
            button.ForeColor = Color.White;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;

            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
        }

        // ================= CẤU HÌNH CONTROL =================

        private void ConfigureControls()
        {
            dtpAppointment.Format = DateTimePickerFormat.Custom;
            dtpAppointment.CustomFormat = "dd/MM/yyyy HH:mm";
            dtpAppointment.ShowUpDown = true;
            dtpAppointment.Value = DateTime.Now.AddMinutes(30);

            cboDoctor.DropDownStyle = ComboBoxStyle.DropDownList;
            cboDoctor.Items.AddRange(new object[]
            {
                "BS. Nguyễn Văn A",
                "BS. Trần Văn B",
                "BS. Lê Thị C"
            });

            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Items.AddRange(new object[]
            {
                "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy"
            });

            cboFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFilterStatus.Items.AddRange(new object[]
            {
                "Tất cả", "Chờ khám", "Đang khám", "Hoàn tất", "Đã hủy"
            });

            cboDoctor.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;
            cboFilterStatus.SelectedIndex = 0;

            dtpFilterDate.Format = DateTimePickerFormat.Short;
            dtpFilterDate.Enabled = false;

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            txtSearch.TextChanged += (s, e) => ApplyFilter();

            chkFilterDate.CheckedChanged += (s, e) =>
            {
                dtpFilterDate.Enabled = chkFilterDate.Checked;
                ApplyFilter();
            };

            dtpFilterDate.ValueChanged += (s, e) => ApplyFilter();

            cboFilterStatus.SelectedIndexChanged +=
                (s, e) => ApplyFilter();

            dgvAppointments.SelectionChanged +=
                DgvAppointments_SelectionChanged;
        }

        // ================= DATAGRIDVIEW =================

        private void ConfigureGrid()
        {
            dgvAppointments.AutoGenerateColumns = false;
            dgvAppointments.AllowUserToAddRows = false;
            dgvAppointments.ReadOnly = true;
            dgvAppointments.MultiSelect = false;

            dgvAppointments.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvAppointments.RowHeadersVisible = false;

            dgvAppointments.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvAppointments.BackgroundColor = Color.White;
            dgvAppointments.BorderStyle = BorderStyle.None;

            dgvAppointments.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            dgvAppointments.GridColor =
                Color.FromArgb(235, 240, 246);

            dgvAppointments.EnableHeadersVisualStyles = false;

            dgvAppointments.ColumnHeadersDefaultCellStyle.BackColor =
                primaryColor;

            dgvAppointments.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgvAppointments.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9F, FontStyle.Bold);

            dgvAppointments.ColumnHeadersHeight = 42;

            dgvAppointments.DefaultCellStyle.Font =
                new Font("Segoe UI", 9F);

            dgvAppointments.DefaultCellStyle.BackColor =
                Color.White;

            dgvAppointments.DefaultCellStyle.ForeColor =
                darkColor;

            dgvAppointments.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 230, 250);

            dgvAppointments.DefaultCellStyle.SelectionForeColor =
                darkColor;

            dgvAppointments.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(244, 248, 253);

            dgvAppointments.RowTemplate.Height = 36;

            AddColumn("AppointmentId", "Mã lịch hẹn");
            AddColumn("PatientName", "Tên bệnh nhân");
            AddColumn("Phone", "Số điện thoại");
            AddColumn("AppointmentDate", "Ngày giờ hẹn");
            AddColumn("Doctor", "Bác sĩ");
            AddColumn("Reason", "Lý do khám");
            AddColumn("Status", "Trạng thái");

            dgvAppointments.Columns["AppointmentDate"]
                .DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
        }

        private void AddColumn(string property, string title)
        {
            DataGridViewTextBoxColumn column =
                new DataGridViewTextBoxColumn();

            column.DataPropertyName = property;
            column.HeaderText = title;
            column.Name = property;

            dgvAppointments.Columns.Add(column);
        }

        // ================= KIỂM TRA DỮ LIỆU =================

        private bool ValidateInput(Appointment current = null)
        {
            errorProvider.Clear();

            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtId.Text))
            {
                errorProvider.SetError(txtId, "Vui lòng nhập mã lịch hẹn!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                errorProvider.SetError(txtName, "Vui lòng nhập họ tên!");
                valid = false;
            }

            if (!Regex.IsMatch(txtPhone.Text.Trim(), @"^(0|\+84)[0-9]{9}$"))
            {
                errorProvider.SetError(txtPhone, "Số điện thoại không hợp lệ!");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                errorProvider.SetError(txtReason, "Vui lòng nhập lý do khám!");
                valid = false;
            }

            DateTime now = DateTime.Now;

            DateTime currentMinute = new DateTime(
                now.Year, now.Month, now.Day,
                now.Hour, now.Minute, 0);

            if (dtpAppointment.Value < currentMinute)
            {
                errorProvider.SetError(
                    dtpAppointment,
                    "Ngày hẹn không được nằm trong quá khứ!");

                valid = false;
            }

            if (appointments.Any(a =>
                string.Equals(
                    a.AppointmentId,
                    txtId.Text.Trim(),
                    StringComparison.OrdinalIgnoreCase)
                && a != current))
            {
                errorProvider.SetError(txtId, "Mã lịch hẹn đã tồn tại!");
                valid = false;
            }

            return valid;
        }

        // ================= THÊM =================

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
                return;

            Appointment appointment = new Appointment();

            appointment.AppointmentId = txtId.Text.Trim();
            appointment.PatientName = txtName.Text.Trim();
            appointment.Phone = txtPhone.Text.Trim();
            appointment.AppointmentDate = dtpAppointment.Value;
            appointment.Doctor = cboDoctor.Text;
            appointment.Reason = txtReason.Text.Trim();
            appointment.Status = cboStatus.Text;

            appointments.Add(appointment);

            ApplyFilter();
            ClearInput();

            MessageBox.Show("Thêm lịch hẹn thành công!",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // ================= CẬP NHẬT =================

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            Appointment appointment = GetSelectedAppointment();

            if (appointment == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần sửa!");
                return;
            }

            if (!ValidateInput(appointment))
                return;

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn cập nhật lịch hẹn này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            appointment.AppointmentId = txtId.Text.Trim();
            appointment.PatientName = txtName.Text.Trim();
            appointment.Phone = txtPhone.Text.Trim();
            appointment.AppointmentDate = dtpAppointment.Value;
            appointment.Doctor = cboDoctor.Text;
            appointment.Reason = txtReason.Text.Trim();
            appointment.Status = cboStatus.Text;

            ApplyFilter();
            ClearInput();

            MessageBox.Show("Cập nhật thành công!");
        }

        // ================= XÓA =================

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            Appointment appointment = GetSelectedAppointment();

            if (appointment == null)
            {
                MessageBox.Show("Vui lòng chọn lịch hẹn cần xóa!");
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa lịch hẹn này?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                appointments.Remove(appointment);

                ApplyFilter();
                ClearInput();

                MessageBox.Show("Xóa lịch hẹn thành công!");
            }
        }

        // ================= CHỌN DÒNG =================

        private Appointment GetSelectedAppointment()
        {
            return bindingSource.Current as Appointment;
        }

        private void DgvAppointments_SelectionChanged(
            object sender, EventArgs e)
        {
            Appointment appointment = GetSelectedAppointment();

            if (appointment == null)
                return;

            txtId.Text = appointment.AppointmentId;
            txtName.Text = appointment.PatientName;
            txtPhone.Text = appointment.Phone;
            dtpAppointment.Value = appointment.AppointmentDate;
            cboDoctor.Text = appointment.Doctor;
            txtReason.Text = appointment.Reason;
            cboStatus.Text = appointment.Status;

            errorProvider.Clear();
        }

        // ================= LÀM MỚI =================

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearInput();
        }

        private void ClearInput()
        {
            txtId.Clear();
            txtName.Clear();
            txtPhone.Clear();
            txtReason.Clear();

            dtpAppointment.Value = DateTime.Now.AddMinutes(30);

            cboDoctor.SelectedIndex = 0;
            cboStatus.SelectedIndex = 0;

            errorProvider.Clear();

            bindingSource.Position = -1;
            dgvAppointments.ClearSelection();
            dgvAppointments.CurrentCell = null;
        }

        // ================= TÌM KIẾM VÀ LỌC =================

        private void ApplyFilter()
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            var result = appointments.Where(a =>
                (a.PatientName ?? "").ToLower().Contains(keyword)
                ||
                (a.Phone ?? "").ToLower().Contains(keyword));

            if (chkFilterDate.Checked)
            {
                DateTime date = dtpFilterDate.Value.Date;

                result = result.Where(a =>
                    a.AppointmentDate.Date == date);
            }

            if (cboFilterStatus.SelectedIndex > 0)
            {
                string status = cboFilterStatus.Text;

                result = result.Where(a => a.Status == status);
            }

            var filtered = result.ToList();

            displayList.RaiseListChangedEvents = false;
            displayList.Clear();

            foreach (Appointment a in filtered)
            {
                displayList.Add(a);
            }

            displayList.RaiseListChangedEvents = true;

            bindingSource.ResetBindings(false);
            bindingSource.Position = -1;

            UpdateStatistics();
        }

        // ================= THỐNG KÊ =================

        private void UpdateStatistics()
        {
            lblTotal.Text = appointments.Count.ToString();

            lblWaiting.Text = appointments.Count(
                a => a.Status == "Chờ khám").ToString();

            lblInProgress.Text = appointments.Count(
                a => a.Status == "Đang khám").ToString();

            lblCompleted.Text = appointments.Count(
                a => a.Status == "Hoàn tất").ToString();

            lblCount.Text = displayList.Count.ToString();
        }

        // ================= XUẤT CSV =================

        private void MenuExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();

            save.Filter = "CSV Files (*.csv)|*.csv";
            save.Title = "Xuất danh sách lịch hẹn";
            save.FileName = "DanhSachLichHen.csv";

            if (save.ShowDialog() == DialogResult.OK)
            {
                StringBuilder csv = new StringBuilder();

                csv.AppendLine(
                    "MaLichHen,HoTen,SoDienThoai,NgayHen,BacSi,LyDo,TrangThai");

                foreach (Appointment a in displayList)
                {
                    csv.AppendLine(string.Join(",",
                        Csv(a.AppointmentId),
                        Csv(a.PatientName),
                        Csv(a.Phone),
                        Csv(a.AppointmentDate.ToString("dd/MM/yyyy HH:mm")),
                        Csv(a.Doctor),
                        Csv(a.Reason),
                        Csv(a.Status)));
                }

                File.WriteAllText(
                    save.FileName,
                    "\uFEFF" + csv.ToString(),
                    Encoding.UTF8);

                MessageBox.Show("Xuất file CSV thành công!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private string Csv(string value)
        {
            return "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";
        }
    }
}
