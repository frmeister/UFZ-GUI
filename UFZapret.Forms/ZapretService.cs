using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace UFZapret.Forms
{
    public static class ZapretService
    {
        private static Process batProcess;
        private static int winwsProcessId = -1;
        private static bool isRunning = false;
        private static Task trackingTask;

        public static bool IsRunning => isRunning;

        public static async Task<bool> Start(string zapretFolder, string configName)
        {
            Debug.WriteLine("=== STARTING ZAPRET ===");

            try
            {
                if (isRunning)
                {
                    Debug.WriteLine("Zapret уже запущен");
                    return false;
                }

                string configPath = Path.Combine(zapretFolder, configName);
                if (!File.Exists(configPath))
                {
                    Debug.WriteLine($"Конфиг не найден: {configPath}");
                    MessageBox.Show($"Конфигурационный файл не найден:\n{configPath}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Проверяем структуру zapret
                if (!ValidateZapretStructure(zapretFolder))
                    return false;

                // Запускаем BAT файл
                Debug.WriteLine("=== Запуск BAT файла ===");
                return await StartBatProcess(configPath, zapretFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка запуска: {ex.Message}");
                MessageBox.Show($"Ошибка запуска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> Stop()
        {
            if (!isRunning)
                return true;

            try
            {
                Debug.WriteLine("=== Остановка Zapret ===");

                // 1. Останавливаем процесс winws.exe по ID
                if (winwsProcessId != -1)
                {
                    try
                    {
                        var winwsProcess = Process.GetProcessById(winwsProcessId);
                        await GracefulShutdown(winwsProcess);
                    }
                    catch (ArgumentException)
                    {
                        // Процесс уже завершен
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка остановки winws.exe: {ex.Message}");
                    }
                }

                // 2. Останавливаем BAT процесс
                if (batProcess != null && !batProcess.HasExited)
                {
                    await GracefulShutdown(batProcess);
                    Debug.WriteLine("BAT процесс завершен");
                }

                // 3. На всякий случай останавливаем все процессы zapret
                KillAllZapretProcesses();

                // 4. Очищаем ссылки
                winwsProcessId = -1;
                batProcess = null;
                isRunning = false;

                // 5. Останавливаем фоновую задачу отслеживания
                if (trackingTask != null && !trackingTask.IsCompleted)
                {
                    // Даем задаче завершиться
                    await Task.WhenAny(trackingTask, Task.Delay(2000));
                }

                Debug.WriteLine("Zapret остановлен");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка остановки: {ex.Message}");
                MessageBox.Show($"Ошибка остановки: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> Toggle(string zapretFolder, string configName)
        {
            if (isRunning)
            {
                return await Stop();
            }
            else
            {
                return await Start(zapretFolder, configName);
            }
        }

        // ===== Вспомогательные методы =====

        private static bool ValidateZapretStructure(string zapretFolder)
        {
            try
            {
                string[] requiredDirs = { "bin", "lists" };
                string[] requiredFilesInBin = { "winws.exe", "service.bat" };

                foreach (var dir in requiredDirs)
                {
                    string dirPath = Path.Combine(zapretFolder, dir);
                    if (!Directory.Exists(dirPath))
                    {
                        MessageBox.Show($"Отсутствует папка: {dir}\n\n" +
                                      "Убедитесь, что вы выбрали правильную папку с zapret.",
                                      "Ошибка структуры", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка проверки структуры: {ex.Message}");
                return false;
            }
        }

        private static async Task<bool> StartBatProcess(string batPath, string workingDir)
        {
            try
            {
                Debug.WriteLine($"Запуск BAT: {batPath}");
                Debug.WriteLine($"Рабочая папка: {workingDir}");

                ProcessStartInfo startInfo;

                if (IsRunningAsAdmin())
                {
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{batPath}\"",
                        WorkingDirectory = workingDir,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    batProcess = Process.Start(startInfo);
                }
                else
                {
                    startInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c \"{batPath}\"",
                        WorkingDirectory = workingDir,
                        Verb = "runas",
                        UseShellExecute = true,
                        CreateNoWindow = false,
                        WindowStyle = ProcessWindowStyle.Normal
                    };

                    batProcess = Process.Start(startInfo);
                }

                if (batProcess != null)
                {
                    batProcess.EnableRaisingEvents = true;
                    batProcess.Exited += (s, e) =>
                    {
                        Debug.WriteLine("BAT процесс завершился");
                        // Если BAT завершился, это не значит, что winws.exe завершился.
                        // Мы не меняем isRunning, потому что winws.exe может работать.
                    };

                    // Ждем, чтобы BAT запустил winws.exe
                    await Task.Delay(2000);

                    // Ищем процесс winws.exe
                    var winwsProcess = await FindWinwsProcessAsync();

                    if (winwsProcess != null)
                    {
                        winwsProcessId = winwsProcess.Id;
                        Debug.WriteLine($"Найден процесс winws.exe (ID: {winwsProcessId})");

                        // Запускаем фоновую задачу для отслеживания завершения winws.exe
                        trackingTask = TrackWinwsProcessAsync(winwsProcessId);

                        isRunning = true;
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Процесс winws.exe не найден после запуска BAT");
                        MessageBox.Show("Не удалось запустить zapret.\n" +
                                      "Процесс winws.exe не был запущен.",
                                      "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка запуска BAT: {ex.Message}");
                MessageBox.Show($"Не удалось запустить конфигурацию:\n{ex.Message}",
                    "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static async Task<Process> FindWinwsProcessAsync()
        {
            try
            {
                // Даем процессу время запуститься
                await Task.Delay(1000);

                // Ищем процесс winws.exe
                var processes = Process.GetProcessesByName("winws");
                if (processes.Length > 0)
                {
                    // Берем самый свежий процесс (последний запущенный)
                    return processes.OrderByDescending(p => p.StartTime).FirstOrDefault();
                }

                // Ищем также в названии "tpws", "dnstls" - другие компоненты zapret
                var allProcesses = Process.GetProcesses();
                foreach (var process in allProcesses)
                {
                    try
                    {
                        if (process.ProcessName.Contains("winws") ||
                            process.ProcessName.Contains("tpws") ||
                            process.ProcessName.Contains("dnstls") ||
                            process.ProcessName.Contains("nfqws"))
                        {
                            return process;
                        }
                    }
                    catch
                    {
                        // Пропускаем процессы, к которым нет доступа
                        continue;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка поиска процесса winws: {ex.Message}");
                return null;
            }
        }

        private static async Task TrackWinwsProcessAsync(int winwsProcessId)
        {
            try
            {
                while (isRunning)
                {
                    await Task.Delay(1000); // Проверяем каждую секунду

                    // Проверяем, существует ли процесс с таким ID
                    bool processExists = false;
                    try
                    {
                        var process = Process.GetProcessById(winwsProcessId);
                        processExists = true;
                        process.Dispose();
                    }
                    catch (ArgumentException)
                    {
                        // Процесс не найден, значит завершен
                        processExists = false;
                    }

                    if (!processExists)
                    {
                        // Процесс завершен
                        Debug.WriteLine("Процесс winws.exe завершился (отслежено в фоне)");
                        isRunning = false;
                        winwsProcessId = -1;

                        // Также убиваем batProcess, если он еще жив
                        if (batProcess != null && !batProcess.HasExited)
                        {
                            try
                            {
                                batProcess.Kill();
                            }
                            catch { }
                        }
                        batProcess = null;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в фоновом отслеживании: {ex.Message}");
            }
        }

        private static bool IsRunningAsAdmin()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        private static async Task GracefulShutdown(Process process)
        {
            if (process == null || process.HasExited)
                return;

            try
            {
                // Пытаемся корректно завершить процесс
                if (!process.CloseMainWindow())
                {
                    // Если не получилось, ждем и принудительно завершаем
                    await Task.Delay(1000);
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                }

                await Task.Run(() => process.WaitForExit(3000));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка graceful shutdown: {ex.Message}");
                try { process.Kill(); } catch { }
            }
        }

        private static void KillAllZapretProcesses()
        {
            string[] processNames = { "winws", "tpws", "dnstls", "zapret", "nfqws", "cmd", "conhost" };

            foreach (string name in processNames)
            {
                try
                {
                    var processes = Process.GetProcessesByName(name);
                    foreach (var process in processes)
                    {
                        // Не убиваем текущие процессы, если они еще живы
                        if (batProcess != null && process.Id == batProcess.Id)
                        {
                            continue;
                        }

                        // Не убиваем winwsProcess по ID, потому что мы его уже убили
                        if (winwsProcessId != -1 && process.Id == winwsProcessId)
                        {
                            continue;
                        }

                        KillProcess(process);
                    }
                }
                catch { }
            }
        }

        private static void KillProcess(Process process)
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(1000);
                }
            }
            catch { }
            finally
            {
                try { process?.Dispose(); } catch { }
            }
        }
    }
}