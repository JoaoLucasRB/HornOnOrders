using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using ModLib;
using System;

namespace HornOnOrders
{
    public class HornOnOrders : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            InformationManager.DisplayMessage(new InformationMessage("Loaded Horn On Movement Orders Successfully!"));
            try
            {
                FileDatabase.Initialise("HornOnOrders");
                HornOnOrdersSettings settings = FileDatabase.Get<HornOnOrdersSettings>(HornOnOrdersSettings.InstanceID);
                if (settings == null) settings = new HornOnOrdersSettings();
                SettingsDatabase.RegisterSettings(settings);
            }
            catch (Exception ex)
            {
                //Handle exceptions
            }
            base.OnSubModuleLoad();
        }

        protected override async void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            bool inMission = Mission.Current != null;
            bool first = true;
            int totalUnits = 0;
            if (inMission)
            {
                bool isMain = Agent.Main == null || !Agent.Main.IsActive();
                if (!isMain)
                {
                    OrderController playerOrderController = Agent.Main.Team.PlayerOrderController;
                    bool notEmptyPlayerOrderController = playerOrderController != null;
                    if(notEmptyPlayerOrderController)
                    {
                        bool haveSelectedFormations = playerOrderController.SelectedFormations.Count<Formation>() > 0;
                        if (haveSelectedFormations)
                        {
                            bool triggerHorn = false;
                            string movement = "";
                            int hornId = 0;
                            foreach(Formation formation in playerOrderController.SelectedFormations)
                            {
                                bool checkMovementOrder = false;
                                if (!this._selectedFormations.ContainsKey(formation.FormationIndex))
                                {
                                    this._selectedFormations.Add(formation.FormationIndex, formation.MovementOrder);
                                    if (first)
                                        totalUnits += formation.Units.Count;
                                    checkMovementOrder = true;
                                } else
                                {
                                    if(this._selectedFormations[formation.FormationIndex] != formation.MovementOrder)
                                    {
                                        this._selectedFormations[formation.FormationIndex] = formation.MovementOrder;
                                        if (first)
                                            totalUnits += formation.Units.Count;
                                        checkMovementOrder = true;
                                    }
                                }
                                if (checkMovementOrder && totalUnits >= HornOnOrdersSettings.Instance.minimumUnits)
                                {
                                    bool isValidFormation = !formation.IsAIControlled && !Mission.Current.GetMissionBehaviour<BattleEndLogic>().PlayerVictory;
                                    if (isValidFormation)
                                    {
                                        if (formation.MovementOrder == MovementOrder.MovementOrderCharge)
                                        {
                                            movement = "charge";
                                            hornId = 693;
                                        }
                                        else if (formation.MovementOrder == MovementOrder.MovementOrderAdvance)
                                        {
                                            movement = "advance";
                                            hornId = 694;
                                        } 
                                        else if (formation.MovementOrder == MovementOrder.MovementOrderFallBack)
                                        {
                                            movement = "fallback";
                                            hornId = 694;
                                        }
                                        else if (formation.MovementOrder == MovementOrder.MovementOrderStop)
                                        {
                                            movement = "stop";
                                            hornId = 694;
                                        } 
                                        else if (formation.MovementOrder == MovementOrder.MovementOrderMove(formation.MovementOrder.GetPosition(formation)))
                                        {
                                            movement = "hold";
                                            hornId = 694;
                                        }
                                        else if (formation.MovementOrder == MovementOrder.MovementOrderFollow(Agent.Main))
                                        {
                                            movement = "follow";
                                            hornId = 694;
                                        }
                                        else if (formation.MovementOrder == MovementOrder.MovementOrderRetreat) {
                                            movement = "retreat";
                                            hornId = 695;
                                        }
                                        if (hornId != 0)
                                        {
                                            triggerHorn = true;
                                        }
                                    }
                                }
                            }
                            if(triggerHorn)
                            {
                                float delayConfig;
                                switch(movement)
                                {
                                    case "charge":
                                        delayConfig = HornOnOrdersSettings.Instance.delayCharge;
                                        break;
                                    case "advance":
                                        delayConfig = HornOnOrdersSettings.Instance.delayAdvance;
                                        break;
                                    case "fallback":
                                        delayConfig = HornOnOrdersSettings.Instance.delayFallback;
                                        break;
                                    case "stop":
                                        delayConfig = HornOnOrdersSettings.Instance.delayStop;
                                        break;
                                    case "hold":
                                        delayConfig = HornOnOrdersSettings.Instance.delayHold;
                                        break;
                                    case "follow":
                                        delayConfig = HornOnOrdersSettings.Instance.delayFollow;
                                        break;
                                    case "retreat":
                                        delayConfig = HornOnOrdersSettings.Instance.delayRetreat;
                                        break;
                                    default:
                                        delayConfig = 1.5f;
                                        break;
                                }
                                int delay = (int) ((float)Math.Round(delayConfig, 1)) * 1000;
                                await Task.Delay(delay);
                                SoundEvent.PlaySound2D(hornId);
                            }
                            first = false;
                        }
                        else
                        {
                            this._selectedFormations.Clear();
                            totalUnits = 0;
                            first = true;
                        }
                    }
                }
            }
        }

        private Dictionary<FormationClass, MovementOrder> _selectedFormations = new Dictionary<FormationClass, MovementOrder>();
    }
}
