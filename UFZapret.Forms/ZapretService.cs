using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UFZapret.Forms
{
    public static class ZapretService
    {
        private static Process winwsProcess;
        private static bool isRunning = false;
        private static CancellationTokenSource trackingCancellationTokenSource;
        private static readonly object processLock = new object();

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

                if (!ValidateZapretStructure(zapretFolder))
                    return false;

                string gameFilter = GetGameFilter(zapretFolder);
                Debug.WriteLine($"GameFilter: {gameFilter}");

                string arguments = await ParseBatArgumentsAsync(configPath, zapretFolder, gameFilter);
                if (string.IsNullOrEmpty(arguments))
                {
                    MessageBox.Show("Не удалось получить аргументы для запуска.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                Debug.WriteLine($"Аргументы winws.exe: {arguments}");

                string winwsPath = Path.Combine(zapretFolder, "bin", "winws.exe");
                return await StartWinwsAsync(winwsPath, arguments, zapretFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка запуска: {ex.Message}");
                MessageBox.Show($"Ошибка запуска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static async Task<bool> Stop(bool force = false)
        {
            if (!isRunning)
                return true;

            try
            {
                Debug.WriteLine("=== Остановка Zapret ===");
                trackingCancellationTokenSource?.Cancel();

                Process processToStop = null;
                lock (processLock)
                {
                    processToStop = winwsProcess;
                }

                if (processToStop != null && !processToStop.HasExited)
                {
                    if (force)
                    {
                        // Принудительная остановка - сразу убиваем процесс
                        Debug.WriteLine("Принудительная остановка процесса");
                        KillProcess(processToStop);
                    }
                    else
                    {
                        // Грейсфул остановка
                        await GracefulShutdown(processToStop);
                    }
                    Debug.WriteLine("Процесс winws.exe завершен");
                }

                KillAllZapretProcesses();

                lock (processLock)
                {
                    winwsProcess = null;
                }
                isRunning = false;

                Debug.WriteLine("Zapret остановлен");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка остановки: {ex.Message}");

                // Даже при ошибке пытаемся убить все процессы
                KillAllZapretProcesses();
                isRunning = false;

                if (!force)
                {
                    MessageBox.Show($"Ошибка остановки: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }

        public static void ForceStop()
        {
            try
            {
                Debug.WriteLine("=== Принудительная остановка Zapret ===");

                // Проверяем, не остановлен ли уже Zapret
                if (!isRunning)
                {
                    Debug.WriteLine("Zapret уже остановлен, пропускаем...");
                    return;
                }

                // Отменяем отслеживание
                trackingCancellationTokenSource?.Cancel();

                // Немедленная остановка всех процессов Zapret
                KillAllZapretProcessesImmediately();

                // Сбрасываем состояние
                isRunning = false;

                lock (processLock)
                {
                    winwsProcess = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка принудительной остановки: {ex.Message}");
            }
        }
        private static void KillAllZapretProcessesImmediately()
        {
            string[] processNames = { "winws", "tpws", "dnstls", "nfqws", "cmd", "conhost" };

            Debug.WriteLine("=== Немедленная остановка всех процессов Zapret ===");

            foreach (string name in processNames)
            {
                try
                {
                    var processes = Process.GetProcessesByName(name);
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                process.Kill();
                                Debug.WriteLine($"Убит процесс: {name} (ID: {process.Id})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Ошибка убийства процесса {name}: {ex.Message}");
                        }
                        finally
                        {
                            process.Dispose();
                        }
                    }
                }
                catch { }
            }
        }

        public static async Task<bool> Toggle(string zapretFolder, string configName)
        {
            return isRunning ? await Stop() : await Start(zapretFolder, configName);
        }

        // ===== Вспомогательные методы =====

        private static bool ValidateZapretStructure(string zapretFolder)
        {
            try
            {
                string[] requiredDirs = { "bin", "lists" };

                foreach (var dir in requiredDirs)
                {
                    string dirPath = Path.Combine(zapretFolder, dir);
                    if (!Directory.Exists(dirPath))
                    {
                        MessageBox.Show($"Отсутствует папка: {dir}\n\nУбедитесь, что выбрали правильную папку с zapret.",
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

        private static async Task EnableTcpTimestampsAsync()
        {
            try
            {
                Debug.WriteLine("=== Включение TCP timestamps ===");

                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = "interface tcp set global timestamps=enabled",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }))
                {
                    await Task.Run(() => process.WaitForExit(3000));
                    Debug.WriteLine(process.ExitCode == 0
                        ? "TCP timestamps включены"
                        : $"Не удалось включить TCP timestamps. Код: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка включения TCP timestamps: {ex.Message}");
            }
        }

        private static string GetGameFilter(string zapretFolder)
        {
            try
            {
                string gameFilterFile = Path.Combine(zapretFolder, "utils", "game_filter.enabled");
                return File.Exists(gameFilterFile) ? "1024-65535" : "12";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка получения GameFilter: {ex.Message}");
                return "12";
            }
        }

        private static async Task<string> ParseBatArgumentsAsync(string batPath, string zapretFolder, string gameFilter)
        {
            try
            {
                Debug.WriteLine($"Парсинг BAT файла: {batPath}");

                string[] lines = File.ReadAllLines(batPath, Encoding.GetEncoding(65001));
                string binPath = Path.Combine(zapretFolder, "bin") + "\\";
                string listsPath = Path.Combine(zapretFolder, "lists") + "\\";

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    if (line.Contains("winws.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        if (line.StartsWith("start", StringComparison.OrdinalIgnoreCase))
                        {
                            int winwsIndex = line.IndexOf("winws.exe", StringComparison.OrdinalIgnoreCase);
                            if (winwsIndex > 0) line = line.Substring(winwsIndex);
                        }

                        int exeEndIndex = line.IndexOf("winws.exe", StringComparison.OrdinalIgnoreCase) + "winws.exe".Length;
                        string args = line.Substring(exeEndIndex).Trim();

                        if (args.StartsWith("\"")) args = args.TrimStart('\"');

                        StringBuilder multiLineArgs = new StringBuilder(args);

                        for (int j = i + 1; j < lines.Length; j++)
                        {
                            string nextLine = lines[j].Trim();
                            if (nextLine.EndsWith("^"))
                            {
                                multiLineArgs.Append(" " + nextLine.TrimEnd('^', ' ').Trim());
                                i = j;
                            }
                            else
                            {
                                multiLineArgs.Append(" " + nextLine);
                                i = j;
                                break;
                            }
                        }

                        string result = multiLineArgs.ToString().Trim();

                        result = result.Replace("\"%BIN%", binPath)
                                     .Replace("\"%LISTS%", listsPath)
                                     .Replace("%BIN%", binPath)
                                     .Replace("%LISTS%", listsPath)
                                     .Replace("%GameFilter%", gameFilter);

                        string basePath = zapretFolder + "\\";
                        if (result.Contains(basePath + basePath))
                            result = result.Replace(basePath + basePath, basePath);

                        result = result.Replace("\"", "").Trim();

                        Debug.WriteLine($"Парсинг завершен: {result}");
                        return result;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка парсинга BAT: {ex.Message}");
                return null;
            }
        }

        private static async Task<bool> StartWinwsAsync(string winwsPath, string arguments, string workingDir)
        {
            try
            {
                Debug.WriteLine($"Запуск winws.exe: {winwsPath}\nАргументы: {arguments}\nРабочая папка: {workingDir}");

                await EnableTcpTimestampsAsync();

                var startInfo = new ProcessStartInfo
                {
                    FileName = winwsPath,
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    UseShellExecute = true,
                    Verb = "runas",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                try
                {
                    lock (processLock)
                    {
                        winwsProcess = Process.Start(startInfo);
                    }
                }
                catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
                {
                    Debug.WriteLine("Пользователь отказался от UAC");
                    MessageBox.Show("Для запуска zapret требуются права администратора.",
                        "Требуются права", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (winwsProcess != null)
                {
                    winwsProcess.EnableRaisingEvents = true;
                    winwsProcess.Exited += (s, e) =>
                    {
                        lock (processLock)
                        {
                            isRunning = false;
                            winwsProcess = null;
                        }
                        Debug.WriteLine("Процесс winws.exe завершился");
                    };

                    isRunning = true;

                    // Даем время процессу запуститься
                    await Task.Delay(1000);

                    if (winwsProcess.HasExited)
                    {
                        Debug.WriteLine($"Winws.exe завершился с кодом: {winwsProcess.ExitCode}");
                        isRunning = false;
                        winwsProcess = null;

                        MessageBox.Show($"Winws.exe завершился с ошибкой.\nВозможно, неверные аргументы или конфигурация.",
                            "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    StartProcessTracking(winwsProcess);
                    Debug.WriteLine($"Процесс winws.exe запущен (ID: {winwsProcess.Id})");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка запуска winws.exe: {ex.Message}");
                MessageBox.Show($"Ошибка запуска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private static void StartProcessTracking(Process process)
        {
            trackingCancellationTokenSource = new CancellationTokenSource();
            var token = trackingCancellationTokenSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(2000, token);

                    try
                    {
                        bool shouldExit = false;
                        lock (processLock)
                        {
                            if (winwsProcess == null || winwsProcess.Id != process.Id)
                                shouldExit = true;
                            else if (process.HasExited)
                            {
                                Debug.WriteLine("Процесс winws.exe завершился (отслеживание)");
                                isRunning = false;
                                winwsProcess = null;
                                shouldExit = true;
                            }
                        }

                        if (shouldExit) break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Ошибка отслеживания процесса: {ex.Message}");
                        break;
                    }
                }
            }, token);
        }

        private static async Task GracefulShutdown(Process process)
        {
            if (process == null || process.HasExited)
                return;

            try
            {
                if (!process.CloseMainWindow())
                {
                    await Task.Delay(1000);
                    if (!process.HasExited) process.Kill();
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
            string[] processNames = { "winws", "tpws", "dnstls", "nfqws" };

            foreach (string name in processNames)
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName(name))
                    {
                        lock (processLock)
                        {
                            if (winwsProcess != null && process.Id == winwsProcess.Id)
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