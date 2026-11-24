using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Lab8_CRUD
{
    public partial class Form1 : Form
    {
        // Имя вашего сервера
        private string connectionString = @"Server=LAPTOP-8FBUS1C9\SQLEXPRESS;Database=StudentDB;Trusted_Connection=True;";

        // Переменная для хранения ID выбранного студента
        private int selectedId = -1;

        public Form1()
        {
            InitializeComponent();
            this.dataGridViewStudents.SelectionChanged += dataGridViewStudents_SelectionChanged;
        }

        // ==================== READ (Загрузка) ====================
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT Id, FullName, Age, GroupName, AverageGrade FROM Students", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dataGridViewStudents.DataSource = dt;

                    // Скрываем колонку Id
                    if (dataGridViewStudents.Columns.Contains("Id"))
                        dataGridViewStudents.Columns["Id"].Visible = false;

                    // ШАГ 7: Счётчик студентов
                    labelStatus.Text = $"Всего студентов: {dt.Rows.Count}";
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка загрузки: " + ex.Message); }
        }

        // ==================== ВЫБОР СТРОКИ ====================
        private void dataGridViewStudents_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridViewStudents.SelectedRows[0];

                if (row.Cells["Id"].Value != null)
                {
                    selectedId = Convert.ToInt32(row.Cells["Id"].Value);
                    textBoxName.Text = row.Cells["FullName"].Value.ToString();
                    numericAge.Value = Convert.ToDecimal(row.Cells["Age"].Value);
                    textBoxGroup.Text = row.Cells["GroupName"].Value.ToString();
                    numericGrade.Value = Convert.ToDecimal(row.Cells["AverageGrade"].Value);
                }
            }
        }

        // ==================== CREATE (Добавление) ====================
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(textBoxName.Text) || string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("Заполните ФИО и Группу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // ШАГ 7: Поиск дубликата перед добавлением
                    string checkSql = "SELECT COUNT(*) FROM Students WHERE FullName = @name";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                    checkCmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Студент с таким ФИО уже есть в базе!", "Дубликат", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // INSERT
                    string sql = "INSERT INTO Students (FullName, Age, GroupName, AverageGrade) VALUES (@name, @age, @group, @grade)";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                    cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                    cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                    cmd.Parameters.AddWithValue("@grade", (double)numericGrade.Value);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Студент успешно добавлен!");
                    LoadData();
                    ClearFields();
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка добавления: " + ex.Message); }
        }

        // ==================== UPDATE (Редактирование) ====================
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите студента в таблице для изменения.", "Ошибка");
                return;
            }
            if (string.IsNullOrWhiteSpace(textBoxName.Text) || string.IsNullOrWhiteSpace(textBoxGroup.Text))
            {
                MessageBox.Show("Заполните ФИО и Группу!", "Ошибка");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE Students SET FullName = @name, Age = @age, GroupName = @group, AverageGrade = @grade WHERE Id = @id";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@name", textBoxName.Text.Trim());
                    cmd.Parameters.AddWithValue("@age", (int)numericAge.Value);
                    cmd.Parameters.AddWithValue("@group", textBoxGroup.Text.Trim());
                    cmd.Parameters.AddWithValue("@grade", (double)numericGrade.Value);
                    cmd.Parameters.AddWithValue("@id", selectedId);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Данные обновлены!");
                    LoadData();
                    ClearFields();
                }
            }
            catch (Exception ex) { MessageBox.Show("Ошибка обновления: " + ex.Message); }
        }

        // ==================== DELETE (Удаление) ====================
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (selectedId == -1)
            {
                MessageBox.Show("Выберите студента для удаления.", "Ошибка");
                return;
            }

            DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить этого студента?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM Students WHERE Id = @id";
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", selectedId);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Студент удален!");
                        LoadData();
                        ClearFields();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Ошибка удаления: " + ex.Message); }
            }
        }

        // ==================== ОЧИСТКА ====================
        private void buttonClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            textBoxName.Clear();
            textBoxGroup.Clear();
            numericAge.Value = 15;
            numericGrade.Value = 0;
            selectedId = -1;
            dataGridViewStudents.ClearSelection();
        }

        // ==================== ШАГ 7: ОТМЕНА ====================
        private void buttonUndo_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearFields();
            MessageBox.Show("Действие отменено (таблица перезагружена).");
        }

        // ==================== ШАГ 7: ЭКСПОРТ В CSV ====================
        private void buttonExport_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.Rows.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV файлы (*.csv)|*.csv";
            sfd.FileName = "StudentsExport.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    // Записываем заголовки
                    string[] columnNames = new string[dataGridViewStudents.Columns.Count];
                    for (int i = 0; i < dataGridViewStudents.Columns.Count; i++)
                        columnNames[i] = dataGridViewStudents.Columns[i].HeaderText;

                    sb.AppendLine(string.Join(";", columnNames));

                    // Записываем строки
                    foreach (DataGridViewRow row in dataGridViewStudents.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            string[] cells = new string[dataGridViewStudents.Columns.Count];
                            for (int i = 0; i < dataGridViewStudents.Columns.Count; i++)
                                cells[i] = row.Cells[i].Value?.ToString();

                            sb.AppendLine(string.Join(";", cells));
                        }
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Экспорт завершен! Файл сохранен.");
                }
                catch (Exception ex) { MessageBox.Show("Ошибка экспорта: " + ex.Message); }
            }
        }
        private void dataGridViewStudents_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}