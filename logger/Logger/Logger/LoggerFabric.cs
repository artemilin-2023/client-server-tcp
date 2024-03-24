using Logger.Abstracts;
using Microsoft.Extensions.Configuration;

namespace Logger
{
    public class LoggerFabric
    {
        private string? file { get; set; }
        private string? directory { get; set; }
        private LogLevel? logLevel { get; set; }

        public LoggerFabric() { }

        /// <summary>
        /// Считывает свойства из файла конфигкрации json формата.
        /// </summary>
        /// <param name="path">Путь до файла конфигурации.</param>
        /// <exception cref="FormatException">Не удалось преобразовать строку из файла в перечисление LogLevel.</exception>
        /// <exception cref="FileNotFoundException">Файл конфигурации не найден.</exception>
        public LoggerFabric SetJsonConfiguration(string path)
        {
            if (!Path.Exists(path))
                throw new FileNotFoundException(nameof(path));

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(path)
                .Build();

            directory = configuration["Logging:Folder"];
            file = configuration["Logging:File"];
            var logLevelString = configuration["Logging:LogLevel"];

            if (logLevelString != null)
            {
                if (!Enum.TryParse(logLevelString, out LogLevel level))
                {
                    throw new FormatException($"The \"{logLevelString}\" could not to be converted to {nameof(LogLevel)}. Check your json configuration.");
                }
                logLevel = level;
            }

            return this;
        }

        /// <summary>
        /// Устанавливает базовую дирректорию.
        /// </summary>
        /// <param name="path">Путь до директории.</param>
        public LoggerFabric SetBasePath(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path);
            directory = path;
            return this;
        }

        /// <summary>
        /// Устанавливает имя файла логов.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        public LoggerFabric SetFile(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);
            file = fileName;
            return this;
        }

        /// <summary>
        /// Устанавливает уровень логирования.
        /// </summary>
        /// <param name="logLevel">Уровень логирования.</param>
        public LoggerFabric SetLogLevel(LogLevel logLevel)
        {
            ArgumentNullException.ThrowIfNull(logLevel);
            this.logLevel = logLevel;
            return this;
        }


        /// <summary>
        /// Собирает объект ILogger в зависимости от конфигурации.
        /// </summary>
        /// <returns>Объект логера.</returns>
        /// <exception cref="Exception">Выбрасывается в случае, если не удалось собрать конфигурацию.</exception>
        public ILogger Build()
        {
            if (file == null && logLevel == null)
                throw new Exception("Use Setups before build");

            if (file != null)
            {
                var fullPath = Path.Combine(directory ?? "", file);
                if (directory != null)
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                return new FileLogger(fullPath, logLevel);
            }
            else
            {
                return new ConsoleLogger(logLevel);
            }
        }

        public object SetLogLevel(object debug)
        {
            throw new NotImplementedException();
        }
    }
}
