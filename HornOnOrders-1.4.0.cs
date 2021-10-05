using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace HornOnOrders
{
    public class HornOnOrders : MBSubModuleBase
    {
        private Dictionary<FormationClass, MovementOrder> _selectedFormations = new Dictionary<FormationClass, MovementOrder>();
        private string[] _supportedOrders = new string[8]
        {
            "battlestart",
            "advance",
            "charge",
            "fallback",
            "follow",
            "move",
            "retreat",
            "stop"
        };
        private Dictionary<string, Dictionary<string, List<string>>> _factionsSoundSet = new Dictionary<string, Dictionary<string, List<string>>>();
        private int _totalUnits = 0;
        private bool _first = true;
        private string _faction = null;
        private float _delayConfig = 1.5f;
        private int _minimumUnits = 40;

        protected override void OnSubModuleLoad()
        {
            this.LoadFilesForFactions();
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Message",
             new TextObject("Message", null),
             9990,
             () => { InformationManager.DisplayMessage(new InformationMessage("Hello World!")); },
             () => { return (false, null); }));
        }

        protected override async void OnApplicationTick(float dt)
        {
            if (!(Mission.Current != null))
                return;
            if(!(Agent.Main == null || !Agent.Main.IsActive()))
            {
                OrderController _playerOrderController = Agent.Main.Team.PlayerOrderController;
                if(_playerOrderController != null) {
                    if (_playerOrderController.SelectedFormations.Count > 0)
                    {
                        bool _triggerHorn = false;
                        string _movement = (string)null;
                        foreach (Formation _selectedFormation in _playerOrderController.SelectedFormations)
                        {
                            bool _checkMovementOrder = false;
                            if (!this._selectedFormations.ContainsKey(_selectedFormation.FormationIndex))
                            {
                                this._selectedFormations.Add(_selectedFormation.FormationIndex, _selectedFormation.GetReadonlyMovementOrderReference());
                                if (this._first)
                                    this._totalUnits += _selectedFormation.CountOfUnits;
                                _checkMovementOrder = true;
                            }
                            else if (this._selectedFormations[_selectedFormation.FormationIndex].OrderEnum.ToString() != _selectedFormation.GetReadonlyMovementOrderReference().OrderEnum.ToString())
                            {
                                this._selectedFormations[_selectedFormation.FormationIndex] = _selectedFormation.GetReadonlyMovementOrderReference();
                                if (this._first)
                                    this._totalUnits += _selectedFormation.CountOfUnits;
                                _checkMovementOrder = true;
                            }
                            if (_checkMovementOrder && (this._totalUnits >= this._minimumUnits))
                            {
                                if (!_selectedFormation.IsAIControlled && !Mission.Current.GetMissionBehaviour<BattleEndLogic>().PlayerVictory)
                                    _movement = _selectedFormation.GetReadonlyMovementOrderReference().OrderEnum.ToString().ToLower();
                                if (_movement != null)
                                    _triggerHorn = true;
                            }
                        }
                        if (_triggerHorn)
                        {
                            if (this._first)
                            {
                                if (Hero.MainHero.Clan.Kingdom != null)
                                    this._faction = Convert.ToString((object)Hero.MainHero.Clan.Kingdom.Name);
                                if (this._faction == null || !this._factionsSoundSet.ContainsKey(this._faction))
                                    this._faction = "Default";
                                this._first = false;
                            }
                            List<string> _filePathList = this._factionsSoundSet[this._faction][_movement];
                            if (_filePathList.Count > 0)
                            {
                                float _totalDelay = (float)Math.Round((double)this._delayConfig, 1) * 1000f;
                                Random random = new Random();
                                int randomIndex = random.Next(_filePathList.Count);
                                using (VorbisWaveReader _vorbisStream = new VorbisWaveReader(_filePathList[randomIndex]))
                                {
                                    using (WaveOutEvent _waveOut = new WaveOutEvent())
                                    {
                                        _waveOut.Init((IWaveProvider)_vorbisStream);
                                        await Task.Delay((int)_totalDelay);
                                        _waveOut.Play();
                                        await Task.Delay((int)_vorbisStream.TotalTime.TotalMilliseconds);
                                    }
                                }
                            }
                        }
                    } 
                    else
                    {
                        this._selectedFormations.Clear();
                        this._totalUnits = 0;
                        this._first = true;
                        this._faction = (string) null;
                    }
                }
            }
        }

        private void LoadFilesForFactions()
        {
            string path = Directory.GetCurrentDirectory().Split(new string[1]
            {
        "bin"
            }, StringSplitOptions.None)[0] + "Modules\\HornOnOrders\\Sounds";
            foreach (string str1 in new List<string>((IEnumerable<string>)Directory.GetDirectories(path)))
            {
                string key = str1.Split(new string[1] { "Sounds\\" }, StringSplitOptions.None)[1];
                Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
                List<string> stringList = new List<string>((IEnumerable<string>)Directory.GetFiles(path + "\\" + key));
                foreach (string supportedOrder in this._supportedOrders)
                {
                    dictionary.Add(supportedOrder, new List<string>());
                    foreach (string str2 in stringList)
                    {
                        if (str2.Split(new string[1] { key + "\\" }, StringSplitOptions.None)[1].Split('_')[0] == supportedOrder)
                            dictionary[supportedOrder].Add(str2);
                    }
                }
                this._factionsSoundSet.Add(key, dictionary);
            }
        }
    }
}
