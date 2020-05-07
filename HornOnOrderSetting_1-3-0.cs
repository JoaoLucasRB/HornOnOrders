
using System.Xml.Serialization;
using ModLib.Definitions;
using ModLib.Definitions.Attributes;

namespace HornOnOrders
{
    public class HornOnOrdersSettings : SettingsBase
    {
        public override string ModName => "Horn On Orders";
        public override string ModuleFolderName => "HornOnOrders";
        public const string InstanceID = "HornOnOrdersSettings";
        [XmlElement]
        public override string ID { get; set; } = InstanceID;
        public static HornOnOrdersSettings Instance
        {
            get
            {
                return (HornOnOrdersSettings)SettingsDatabase.GetSettings<HornOnOrdersSettings>();
            }
        }
        [XmlElement]
        [SettingProperty("Delay after Charge Order", 0f, 5f, "Delay before the horn plays in seconds after a Charge order")]
        public float delayCharge { get; set; } = 1.5f;
        [XmlElement]
        [SettingProperty("Delay after Advance Order", 0f, 5f, "Delay before the horn plays in seconds after a Advance order")]
        public float delayAdvance { get; set; } = 1.5f;
        [XmlElement]
        [SettingProperty("Delay after Fallback Order", 0f, 5f, "Delay before the horn plays in seconds after a Fallback order")]
        public float delayFallback { get; set; } = 1.5f;
        [XmlElement]
        [SettingProperty("Delay after Stop Order", 0f, 5f, "Delay before the horn plays in seconds after a Stop order")]
        public float delayStop { get; set; } = 1.5f;
        [XmlElement]
        [SettingProperty("Delay after Hold Position Order", 0f, 5f, "Delay before the horn plays in seconds after a Hold Position order")]
        public float delayHold { get; set; } = 1.5f;
        [XmlElement]
        [SettingProperty("Delay after Follow Order", 0f, 5f, "Delay before the horn plays in seconds after a Follow order")]
        public float delayFollow { get; set; } = 1.5f;
        [XmlElement]
        [SettingProperty("Delay after Retreat Order", 0f, 5f, "Delay before the horn plays in seconds after a Retreat order")]
        public float delayRetreat { get; set; } = 1.5f;

        [XmlElement]
        [SettingProperty("Minimum amount of units", 1, 250, "Minimum amount of units needed")]
        public int minimumUnits { get; set; } = 40;
    }
}
