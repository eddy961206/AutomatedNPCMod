using StardewModdingAPI;

namespace AutomatedNPCMod.Core
{
    /// <summary>
    /// 모드 설정을 관리하는 클래스.
    /// </summary>
    public class ConfigManager
    {
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;
        private ModConfig _config;

        public ConfigManager(IModHelper helper, IMonitor monitor)
        {
            _helper = helper;
            _monitor = monitor;
        }

        /// <summary>
        /// 모드 설정을 로드합니다.
        /// </summary>
        public void LoadConfig()
        {
            _config = _helper.ReadConfig<ModConfig>();
            _monitor.Log("모드 설정 로드 완료.", LogLevel.Debug);
        }

        /// <summary>
        /// 현재 모드 설정을 반환합니다.
        /// </summary>
        public ModConfig GetConfig()
        {
            return _config;
        }

        /// <summary>
        /// 모드 설정을 저장합니다.
        /// </summary>
        public void SaveConfig()
        {
            _helper.WriteConfig(_config);
            _monitor.Log("모드 설정 저장 완료.", LogLevel.Debug);
        }
    }

    /// <summary>
    /// 모드 설정을 정의하는 클래스.
    /// </summary>
    public class ModConfig
    {
        public bool EnableAutomatedNPCs { get; set; } = true;
        public float ProfitMultiplier { get; set; } = 1.0f;
        public int MaxNPCs { get; set; } = 5;

        public ModConfig()
        {
            // 기본 설정값
        }
    }
}

