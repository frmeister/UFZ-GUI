using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    public partial class FormConfiguration : Form
    {
        string folderPath;
        string configName;
        private List<Button> configButtons; // Список всех кнопок
        private List<string> batFiles; // Список найденных .bat файлов
        private CancellationTokenSource _autoConfigCancellation;

        public FormConfiguration()
        {
            InitializeComponent();

            InitializeButtonsList();

            CfgButtonsDisable();

            DrawCurrentPath();

            this.Load += FormConfiguration_Load;

            StatusDownload_Origin();

            // Кнопка отмены
            var cancelButton = new Button
            {
                Text = "Отмена",
                Location = new Point(config_buttonAutoCfg.Right + 10, config_buttonAutoCfg.Top),
                Size = config_buttonAutoCfg.Size,
                Visible = false
            };

            cancelButton.Click += (s, e) =>
            {
                _autoConfigCancellation?.Cancel();
            };

            this.Controls.Add(cancelButton);

            config_buttonAutoCfg.EnabledChanged += (s, e) =>
            {
                cancelButton.Visible = !config_buttonAutoCfg.Enabled;
            };

        }

        private async void FormConfiguration_Load(object sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        #region LOGIC
        private void InitializeButtonsList()
        {
            configButtons = new List<Button>
            {
                config_button1, config_button2, config_button3, config_button4,
                config_button5, config_button6, config_button7, config_button8,
                config_button9, config_button10, config_button11, config_button12,
                config_button13, config_button14, config_button15, config_button16,
                config_button17, config_button18, config_button19, config_button20,
                config_button21, config_button22, config_button23, config_button24,
            };

            foreach (var button in configButtons)
            {
                button.Click += ConfigButton_Click;
            }
        }

        // Disabling all func buttons for configs inside zapret
        private void CfgButtonsDisable()
        {
            foreach (var button in configButtons)
            {
                button.Enabled = false;
                button.Visible = false;
                button.Text = ""; // Очищаем текст
                button.Tag = null; // Очищаем Tag (там будет путь к файлу)
            }
        }

        private void CfgButtonsEnableAndFill(List<string> files)
        {
            CfgButtonsDisable();

            for (int i = 0; i < Math.Min(files.Count, configButtons.Count); i++)
            {
                string filePath = files[i];
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                configButtons[i].Text = fileName;
                configButtons[i].Tag = filePath;
                configButtons[i].Visible = true;
                configButtons[i].Enabled = true;

                // Проверяем, должна ли эта кнопка быть выбрана
                if (configName != "none")
                {
                    string configNameWithoutExtension = configName.Replace(".bat", "");
                    if (fileName == configNameWithoutExtension)
                    {
                        configButtons[i].Enabled = false;
                        configButtons[i].BackColor = Color.LightGray;
                    }
                }
            }
        }

        public List<string> GetBatFiles(string folderPath)
        {
            try
            {
                // Проверяем существование папки
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show($"Папка не найдена: {folderPath}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return new List<string>();
                }

                // Получаем все .bat файлы
                string[] batFiles = Directory.GetFiles(folderPath, "*.bat");

                if (batFiles.Length == 0)
                {
                    MessageBox.Show($"В папке не найдены .bat файлы:\n{folderPath}",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return new List<string>();
                }

                // Преобразуем в список и сортируем по имени
                return batFiles.OrderBy(f => f).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файлов:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }

        private void DrawCurrentPath()
        {
            folderPath = ConfigManager.GetValue("pathOrigin", "none");
            configName = ConfigManager.GetValue("currentConfig", "none");

            if (folderPath != "none" && Directory.Exists(folderPath))
            {
                // Получаем список файлов и сохраняем в batFiles
                batFiles = GetBatFiles(folderPath);

                // Заполняем кнопки
                CfgButtonsEnableAndFill(batFiles);

                UpdateStatus_Config($"Найдено конфигов: {batFiles.Count}", 0);

                // Теперь ищем и деактивируем выбранный конфиг
                if (configName != "none" && configName != "")
                {
                    string configNameWithoutExtension = configName.Replace(".bat", "");

                    foreach (var button in configButtons)
                    {
                        if (button.Visible && button.Text == configNameWithoutExtension)
                        {
                            button.Enabled = false;
                            button.BackColor = Color.LightGray;
                            UpdateStatus_Config($"\nВыбран конфиг: {button.Text}", 1);
                            break;
                        }
                    }
                }
            }
            else
            {
                UpdateStatus_Config("Папка Zapret не настроена. Выберите папку.", 0);
            }
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                config_buttonUpdate.Enabled = false;
                UpdateStatus_Info("Проверка обновлений...", 0);
                Application.DoEvents(); // Обновляем UI

                // Проверяем, указан ли путь
                if (string.IsNullOrEmpty(folderPath) || folderPath == "none" || !Directory.Exists(folderPath))
                {
                    UpdateStatus_Info("\n$Путь к zapret не указан", 0);
                    return;
                }

                // Проверяем обновление
                bool updateAvailable = await DataService.IsThereUpdateZapret_OriginAsync(folderPath, true);

                Debug.WriteLine($"[FormConfiguration] Результат проверки: {updateAvailable}");

                if (updateAvailable)
                {
                    UpdateStatus_Info("\n$Доступно обновление!", 0);
                    config_buttonUpdate.Enabled = true;
                }
                else
                {
                    UpdateStatus_Info("\n$Установлена последняя версия", 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FormConfiguration] Ошибка в CheckForUpdatesAsync: {ex.Message}");
                UpdateStatus_Info($"\n$Ошибка: {ex.Message}", 0);
            }
        }



        #endregion

        #region STATUS

        private void UpdateStatus_Info(string text, int row)
        {
            switch (row)
            {
                case 0:
                    config_textBoxInfo.Text = text;
                    break;
                case 1:
                    config_textBoxInfo.Text += text;
                    break;
            }
        }

        private void UpdateStatus_Config(string text, int row)
        {
            switch (row)
            {
                case 0:
                    config_textBoxConfigMaster.Text = text;
                    break;
                case 1:
                    config_textBoxConfigMaster.Text += text;
                    break;
            }
        }

        private void StatusDownload_Origin()
        {
            if (ConfigManager.GetValue("originPath", "none") != "none")
            {
                config_buttonDownload.Enabled = true;
                config_buttonDownload.Visible = true;
            }
        }

        #endregion

        #region AUTO-CONFIG SEARCH

        private async Task<string> FindWorkingConfigWithProgress(IProgress<string> progress, CancellationToken cancellationToken)
        {
            try
            {
                // Проверяем наличие файлов
                if (batFiles == null || batFiles.Count == 0)
                {
                    progress?.Report("Конфиги не найдены");
                    return null;
                }

                // Останавливаем текущий запущенный конфиг, если есть
                if (ZapretService.IsRunning)
                {
                    progress?.Report("Останавливаем текущий конфиг...");
                    await ZapretService.Stop();
                    await Task.Delay(1000, cancellationToken);
                }

                progress?.Report($"Найдено конфигов: {batFiles.Count}");
                await Task.Delay(500, cancellationToken);

                string workingConfig = null;
                int tested = 0;

                foreach (var configFile in batFiles)
                {
                    // Проверяем отмену
                    cancellationToken.ThrowIfCancellationRequested();

                    tested++;
                    string fileName = Path.GetFileName(configFile);
                    progress?.Report($"Тестируем ({tested}/{batFiles.Count}): {fileName}");

                    // Тестируем конфиг
                    bool isWorking = await ZapretService.TestConfigAsync(folderPath, fileName);

                    if (isWorking)
                    {
                        workingConfig = fileName;
                        progress?.Report($"✓ Найден рабочий конфиг: {fileName}");
                        break;
                    }

                    progress?.Report($"✗ Конфиг не работает: {fileName}");

                    // Пауза между тестами
                    await Task.Delay(2000, cancellationToken);
                }

                return workingConfig;
            }
            catch (OperationCanceledException)
            {
                // Гарантированно останавливаем zapret при отмене
                await ZapretService.Stop();
                throw;
            }
            catch (Exception ex)
            {
                progress?.Report($"Ошибка: {ex.Message}");
                return null;
            }
        }

        private async void config_buttonAutoCfg_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверяем, указан ли путь к zapret
                string pathOrigin = ConfigManager.GetValue("pathOrigin", "none");
                if (pathOrigin == "none" || !Directory.Exists(pathOrigin))
                {
                    MessageBox.Show("Сначала укажите путь к zapret в настройках!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Проверяем наличие конфигов
                if (batFiles == null || batFiles.Count == 0)
                {
                    MessageBox.Show("Конфиги не найдены в указанной папке!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Блокируем кнопку на время поиска
                config_buttonAutoCfg.Enabled = false;
                config_buttonAutoCfg.Text = "Поиск...";

                // Создаем токен отмены
                _autoConfigCancellation = new CancellationTokenSource();

                // Показываем прогресс
                var progress = new Progress<string>(message =>
                {
                    config_labelStatus.Text = message;
                    config_labelStatus.Refresh();
                });

                // Запускаем поиск асинхронно
                string workingConfig = await FindWorkingConfigWithProgress(progress, _autoConfigCancellation.Token);

                // Обрабатываем результат
                if (!string.IsNullOrEmpty(workingConfig))
                {
                    // Сохраняем найденный конфиг
                    ConfigManager.SetValue("currentConfig", workingConfig);

                    // Обновляем интерфейс
                    DrawCurrentPath();

                    MessageBox.Show($"Найден рабочий конфиг: {workingConfig}\n\nКонфиг сохранен и выбран.",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось найти рабочий конфиг.\n\nПопробуйте:\n" +
                        "1. Проверить подключение к интернету\n" +
                        "2. Обновить конфиги через кнопку 'Обновить'\n" +
                        "3. Выбрать конфиг вручную",
                        "Результат поиска", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Поиск конфига отменен", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске конфига: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Восстанавливаем кнопку
                config_buttonAutoCfg.Enabled = true;
                config_buttonAutoCfg.Text = "Авто-подбор конфига";
                config_labelStatus.Text = "Готово";
                _autoConfigCancellation?.Dispose();
                _autoConfigCancellation = null;
            }
        }

        #endregion

        #region BUTTONS
        private void ConfigButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;

            // Проверяем, что это кнопка конфига (по имени)
            if (clickedButton == null || !clickedButton.Name.StartsWith("config_button") ||
                clickedButton.Name.EndsWith("Save") || clickedButton.Name.EndsWith("Cancel") ||
                clickedButton.Name.EndsWith("ChangeCfg") || clickedButton.Name.EndsWith("Update") ||
                clickedButton.Name.EndsWith("AutoCfg"))
                return;

            // 1. Сначала активируем ВСЕ остальные кнопки (снимаем предыдущий выбор)
            foreach (var button in configButtons)
            {
                // Пропускаем нажатую кнопку
                if (button == clickedButton) continue;

                // Активируем все остальные кнопки
                if (button.Visible && !button.Enabled)
                {
                    button.Enabled = true;
                    button.BackColor = SystemColors.Control; // Возвращаем стандартный цвет
                }
            }

            // 2. Деактивируем только нажатую кнопку
            clickedButton.Enabled = false;

            // 3. Сохраняем выбор
            configName = clickedButton.Text + ".bat";

            // 4. Показываем информацию о выборе
            UpdateStatus_Config($"\nВыбран конфиг:\n{clickedButton.Text}", 1);
        }

        private void config_buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                DataService.UpdateZapret_Origin(folderPath);

                MessageBox.Show(
                    "$Обновление установлено успешно!",
                    "$Обновление",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
            catch
            {
                MessageBox.Show(
                    "$Не удалось установить обновление",
                    "$Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void config_buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Saving the folder path into Config.cfg
                DataService.SaveFolderPath(folderPath);

                DataService.SaveCurrentConfig(configName);

                FormMain formMain = new FormMain(false);
                formMain.CheckIsConfigAvalible();
            }
            catch
            {
                MessageBox.Show("Error", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void config_buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void config_buttonChangeCfg_Click(object sender, EventArgs e)
        {
            config_folderBrowserDialogCfg.ShowDialog();
            folderPath = config_folderBrowserDialogCfg.SelectedPath;

            CfgButtonsEnableAndFill(GetBatFiles(folderPath));
        }

        private void config_buttonDownload_Click(object sender, EventArgs e)
        {
            config_folderBrowserDialogCfg.ShowDialog();
            folderPath = config_folderBrowserDialogCfg.SelectedPath;

            DataService.CreateNewGitClone_Zapret(folderPath);

            if (!DataService.GitExisting_Zapret(folderPath))
            {
                MessageBox.Show("Произошла ошибка, файлы не установлены!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
            else
            {
                ConfigManager.SetValue("pathOrigin", folderPath);

                config_buttonDownload.Enabled = false;
                config_buttonDownload.Visible = false;
            }
        }

        #endregion
    }
}