﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Chromatics.Controllers;
using Chromatics.Datastore;
using Chromatics.DeviceInterfaces;
using Chromatics.LCDInterfaces;
using Cyotek.Windows.Forms;
using Microsoft.VisualBasic.FileIO;

namespace Chromatics
{
    partial class Chromatics : ILogWrite
    {
        private readonly DataGridViewComboBoxColumn _dGmode = new DataGridViewComboBoxColumn();

        private readonly Dictionary<BulbModeTypes, string> _bulbModes = new Dictionary<BulbModeTypes, string>
        {
            //Keys
            {BulbModeTypes.Disabled, "Disabled"},
            {BulbModeTypes.Standby, "Standby"},
            {BulbModeTypes.DefaultColor, "Default Color"},
            {BulbModeTypes.HighlightColor, "Highlight Colour"},
            {BulbModeTypes.EnmityTracker, "Enmity Tracker"},
            {BulbModeTypes.TargetHp, "Target HP"},
            {BulbModeTypes.StatusEffects, "Status Effects"},
            {BulbModeTypes.HpTracker, "HP Tracker"},
            {BulbModeTypes.MpTracker, "MP Tracker"},
            {BulbModeTypes.TpTracker, "TP Tracker"},
            {BulbModeTypes.Castbar, "Castbar"},
            {BulbModeTypes.DutyFinder, "Duty Finder Bell"},
            {BulbModeTypes.ChromaticsDefault, "Chromatics Default"}
        };

        private readonly Dictionary<DevModeTypes, string> _devModesA = new Dictionary<DevModeTypes, string>
        {
            //Keys
            {DevModeTypes.Disabled, "Disabled"},
            //{DevModeTypes.Standby, "Standby"},
            {DevModeTypes.DefaultColor, "Default Color"},
            {DevModeTypes.HighlightColor, "Highlight Colour"},
            {DevModeTypes.EnmityTracker, "Enmity Tracker"},
            {DevModeTypes.TargetHp, "Target HP"},
            {DevModeTypes.HpTracker, "HP Tracker"},
            {DevModeTypes.MpTracker, "MP Tracker"},
            {DevModeTypes.TpTracker, "TP Tracker"},
            {DevModeTypes.Castbar, "Castbar"},
            {DevModeTypes.DutyFinder, "Duty Finder Bell"}
        };

        private readonly Dictionary<LightbarMode, string> _lightbarModes = new Dictionary<LightbarMode, string>
        {
            //Keys
            {LightbarMode.Disabled, "Disabled"},
            {LightbarMode.DefaultColor, "Default Color"}, //
            {LightbarMode.HighlightColor, "Highlight Colour"}, //
            {LightbarMode.EnmityTracker, "Enmity Tracker"}, //
            {LightbarMode.TargetHp, "Target HP"}, //
            {LightbarMode.HpTracker, "HP Tracker"}, //
            {LightbarMode.MpTracker, "MP Tracker"}, //
            {LightbarMode.TpTracker, "TP Tracker"}, //
            {LightbarMode.Castbar, "Castbar"}, //
            {LightbarMode.DutyFinder, "Duty Finder Bell"},
            {LightbarMode.CurrentExp, "Experience Tracker"},
            {LightbarMode.JobGauge, "Job Gauge"}
        };

        private readonly Dictionary<string, string[]> _mappingPalette = new Dictionary<string, string[]>
        {
            //Keys
            {"ColorMappingBaseColor", new[] {"Base Color", "1", "Black", "White"}},
            {"ColorMappingHighlightColor", new[] {"Highlight Color", "1", "Black", "White"}},
            {"ColorMappingHpFull", new[] {"HP Full", "2", "Black", "White"}},
            {"ColorMappingHpEmpty", new[] {"HP Empty", "2", "Black", "White"}},
            {"ColorMappingHpCritical", new[] {"HP Critical", "2", "Black", "White"}},
            {"ColorMappingHpLoss", new[] {"HP Loss", "2", "Black", "White"}},
            {"ColorMappingMpFull", new[] {"MP Full", "2", "Black", "White"}},
            {"ColorMappingMpEmpty", new[] {"MP Empty", "2", "Black", "White"}},
            {"ColorMappingTpFull", new[] {"TP Full", "2", "Black", "White"}},
            {"ColorMappingTpEmpty", new[] {"TP Empty", "2", "Black", "White"}},
            {"ColorMappingGpFull", new[] {"GP Full", "2", "Black", "White"}},
            {"ColorMappingGpEmpty", new[] {"GP Empty", "2", "Black", "White"}},
            {"ColorMappingCpFull", new[] {"CP Full", "2", "Black", "White"}},
            {"ColorMappingCpEmpty", new[] {"CP Empty", "2", "Black", "White"}},
            {"ColorMappingExpEmpty", new[] {"Experience Bar (Empty)", "2", "Black", "White"}},
            {"ColorMappingExpFull", new[] { "Experience Bar (Full)", "2", "Black", "White"}},
            {"ColorMappingCastChargeFull", new[] {"Cast Charge", "3", "Black", "White"}},
            {"ColorMappingCastChargeEmpty", new[] {"Cast Empty", "3", "Black", "White"}},
            {"ColorMappingEmnity0", new[] {"Minimal Enmity", "4", "Black", "White"}},
            {"ColorMappingEmnity1", new[] {"Low Enmity", "4", "Black", "White"}},
            {"ColorMappingEmnity2", new[] {"Medium Enmity", "4", "Black", "White"}},
            {"ColorMappingEmnity3", new[] {"High Enmity", "4", "Black", "White"}},
            {"ColorMappingEmnity4", new[] {"Max Enmity", "4", "Black", "White"}},
            {"ColorMappingNoEmnity", new[] {"No Enmity", "4", "Black", "White"}},
            {"ColorMappingTargetHpClaimed", new[] {"Target HP - Claimed", "5", "Black", "White"}},
            {"ColorMappingTargetHpIdle", new[] {"Target HP - Idle", "5", "Black", "White"}},
            {"ColorMappingTargetHpEmpty", new[] {"Target HP - Empty", "5", "Black", "White"}},
            {"ColorMappingTargetCasting", new[] {"Target Casting", "5", "Black", "White"}},
            {"ColorMappingBind", new[] {"Bind", "6", "Black", "White"}},
            {"ColorMappingPetrification", new[] {"Petrification", "6", "Black", "White"}},
            {"ColorMappingSlow", new[] {"Slow", "6", "Black", "White"}},
            {"ColorMappingStun", new[] {"Stun", "6", "Black", "White"}},
            {"ColorMappingSilence", new[] {"Silence", "6", "Black", "White"}},
            {"ColorMappingPoison", new[] {"Poison", "6", "Black", "White"}},
            {"ColorMappingPollen", new[] {"Pollen", "6", "Black", "White"}},
            {"ColorMappingPox", new[] {"Pox", "6", "Black", "White"}},
            {"ColorMappingParalysis", new[] {"Paralysis", "6", "Black", "White"}},
            {"ColorMappingLeaden", new[] {"Leaden", "6", "Black", "White"}},
            {"ColorMappingIncapacitation", new[] {"Incapacitation", "6", "Black", "White"}},
            {"ColorMappingDropsy", new[] {"Dropsy", "6", "Black", "White"}},
            {"ColorMappingAmnesia", new[] {"Amnesia", "6", "Black", "White"}},
            {"ColorMappingBleed", new[] {"Bleed", "6", "Black", "White"}},
            {"ColorMappingMisery", new[] {"Misery", "6", "Black", "White"}},
            {"ColorMappingSleep", new[] {"Sleep", "6", "Black", "White"}},
            {"ColorMappingDaze", new[] {"Daze", "6", "Black", "White"}},
            {"ColorMappingHeavy", new[] {"Heavy", "6", "Black", "White"}},
            {"ColorMappingInfirmary", new[] {"Infirmary", "6", "Black", "White"}},
            {"ColorMappingBurns", new[] {"Burns", "6", "Black", "White"}},
            {"ColorMappingDeepFreeze", new[] {"Deep Freeze", "6", "Black", "White"}},
            {"ColorMappingDamageDown", new[] {"Damage Down", "6", "Black", "White"}},
            {"ColorMappingGcdHot", new[] {"GCD Countdown Hot", "7", "Black", "White"}},
            {"ColorMappingGcdReady", new[] {"GCD Countdown Ready", "7", "Black", "White"}},
            {"ColorMappingGcdEmpty", new[] {"GCD Countdown Empty", "7", "Black", "White"}},
            {"ColorMappingHotbarProc", new[] {"Keybind Proc/Combo", "7", "Black", "White"}},
            {"ColorMappingHotbarCd", new[] {"Keybind Cooldown", "7", "Black", "White"}},
            {"ColorMappingHotbarReady", new[] {"Keybind Ready", "7", "Black", "White"}},
            {"ColorMappingHotbarOutRange", new[] {"Keybind Out of Range", "7", "Black", "White"}},
            {"ColorMappingHotbarNotAvailable", new[] {"Keybind Not Available", "7", "Black", "White"}},
            {"ColorMappingDutyFinderBell", new[] {"Duty Finder Bell", "8", "Black", "White"}},
            {"ColorMappingJobWARNegative", new[] {"WAR: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobWARWrathBurst", new[] { "WAR: Wrath (Burst)", "9", "Black", "White"}},
            {"ColorMappingJobWARWrathMax", new[] { "WAR: Wrath (Full)", "9", "Black", "White"}},
            {"ColorMappingJobPLDNegative", new[] {"PLD: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobPLDShieldOath", new[] {"PLD: Shield Oath", "9", "Black", "White"}},
            {"ColorMappingJobPLDSwordOath", new[] { "PLD: Sword Oath", "9", "Black", "White"}},
            {"ColorMappingJobMNKNegative", new[] {"MNK: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobMNKGreased", new[] { "MNK: Greased Lighting", "9", "Black", "White"}},
            {"ColorMappingJobDRGNegative", new[] {"DRG: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobDRGBloodDragon", new[] {"DRG: Blood of the Dragon", "9", "Black", "White"}},
            {"ColorMappingJobBRDNegative", new[] {"BRD: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobBRDRepertoire", new[] { "BRD: Repertoire Stack", "9", "Black", "White"}},
            {"ColorMappingJobBRDBallad", new[] { "BRD: Mage's Ballad", "9", "Black", "White"}},
            {"ColorMappingJobBRDArmys", new[] { "BRD: Army's Paeon", "9", "Black", "White"}},
            {"ColorMappingJobBRDMinuet", new[] { "BRD: The Wanderers' Minuet", "9", "Black", "White"}},
            {"ColorMappingJobWHMNegative", new[] {"WHM: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobWHMFlowerPetal", new[] {"WHM: Flower", "9", "Black", "White"}},
            {"ColorMappingJobWHMFreecure", new[] {"WHM: Freecure Proc", "9", "Black", "White"}},
            {"ColorMappingJobBLMNegative", new[] {"BLM: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobBLMAstralFire", new[] { "BLM: Astral Fire", "9", "Black", "White"}},
            {"ColorMappingJobBLMUmbralIce", new[] { "BLM: Umbral Ice", "9", "Black", "White"}},
            {"ColorMappingJobBLMUmbralHeart", new[] { "BLM: Umbral Heart", "9", "Black", "White"}},
            {"ColorMappingJobBLMEnochian", new[] { "BLM: Enochian", "9", "Black", "White"}},
            {"ColorMappingJobSMNNegative", new[] {"SMN: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobSMNAetherflow", new[] {"SMN: Aetherflow", "9", "Black", "White"}},
            {"ColorMappingJobSCHNegative", new[] {"SCH: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobSCHAetherflow", new[] {"SCH: Aetherflow", "9", "Black", "White"}},
            {"ColorMappingJobNINNegative", new[] {"NIN: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobNINHuton", new[] {"NIN: Huton", "9", "Black", "White"}},
            {"ColorMappingJobDRKNegative", new[] {"DRK: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobDRKBloodGauge", new[] {"DRK: Blood Gauge", "9", "Black", "White"}},
            {"ColorMappingJobDRKGrit", new[] {"DRK: Grit", "9", "Black", "White"}},
            {"ColorMappingJobDRKDarkside", new[] {"DRK: Darkside", "9", "Black", "White"}},
            {"ColorMappingJobASTNegative", new[] {"AST: Blank Key" , "9", "Black", "White"}},
            {"ColorMappingJobASTArrow", new[] { "AST: Arrow Drawn", "9", "Black", "White"}},
            {"ColorMappingJobASTBalance", new[] { "AST: Balance Drawn", "9", "Black", "White"}},
            {"ColorMappingJobASTBole", new[] { "AST: Bole Drawn", "9", "Black", "White"}},
            {"ColorMappingJobASTEwer", new[] { "AST: Ewer Drawn", "9", "Black", "White"}},
            {"ColorMappingJobASTSpear", new[] { "AST: Spear Drawn", "9", "Black", "White"}},
            {"ColorMappingJobASTSpire", new[] { "AST: Spire Drawn", "9", "Black", "White"}},
            {"ColorMappingJobMCHNegative", new[] {"MCH: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobMCHAmmo", new[] { "MCH: Ammo", "9", "Black", "White"}},
            {"ColorMappingJobMCHHeatGauge", new[] { "MCH: Heat Gauge", "9", "Black", "White"}},
            {"ColorMappingJobMCHOverheat", new[] { "MCH: Overheat", "9", "Black", "White"}},
            {"ColorMappingJobSAMNegative", new[] {"SAM: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobSAMSetsu", new[] { "SAM: Setsu", "9", "Black", "White"}},
            {"ColorMappingJobSAMGetsu", new[] { "SAM: Getsu", "9", "Black", "White"}},
            {"ColorMappingJobSAMKa", new[] { "SAM: Ka", "9", "Black", "White"}},
            {"ColorMappingJobSAMKenki", new[] { "SAM: Kenki Charge", "9", "Black", "White"}},
            {"ColorMappingJobRDMNegative", new[] {"RDM: Blank Key", "9", "Black", "White"}},
            {"ColorMappingJobRDMBlackMana", new[] { "RDM: Black Mana", "9", "Black", "White"}},
            {"ColorMappingJobRDMWhiteMana", new[] { "RDM: White Mana", "9", "Black", "White"}},

            {"ColorMappingWeatherClearBase", new[] { "Clear Skies (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherClearHighlight", new[] { "Clear Skies (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherFairBase", new[] { "Fair Skies (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherFairHighlight", new[] { "Fair Skies (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherCloudsBase", new[] { "Clouds (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherCloudsHighlight", new[] { "Clouds (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherFogBase", new[] { "Fog (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherFogHighlight", new[] { "Fog (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherWindBase", new[] { "Wind (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherWindHighlight", new[] { "Wind (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherGalesBase", new[] { "Gales (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherGalesHighlight", new[] { "Gales (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherRainBase", new[] { "Rain (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherRainHighlight", new[] { "Rain (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherShowersBase", new[] { "Showers (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherShowersHighlight", new[] { "Showers (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherThunderBase", new[] { "Thunder (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherThunderHighlight", new[] { "Thunder (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherThunderstormsBase", new[] { "Thunderstorms (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherThunderstormsHighlight", new[] { "Thunderstorms (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherDustBase", new[] { "Dust Storms (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherDustHighlight", new[] { "Dust Storms (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherSandstormBase", new[] { "Sandstorms (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherSandstormHighlight", new[] { "Sandstorms (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherHotspellBase", new[] { "Hot Spells (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherHotspellHighlight", new[] { "Hot Spells (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherHeatwaveBase", new[] { "Heat Waves (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherHeatwaveHighlight", new[] { "Heat Waves (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherSnowBase", new[] { "Snow (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherSnowHighlight", new[] { "Snow (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherBlizzardsBase", new[] { "Blizzards (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherBlizzardsHighlight", new[] { "Blizzards (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherGloomBase", new[] { "Gloom (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherGloomHighlight", new[] { "Gloom (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherAurorasBase", new[] { "Auroras (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherAurorasHighlight", new[] { "Auroras (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherDarknessBase", new[] { "Darkness (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherDarknessHighlight", new[] { "Darkness (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherTensionBase", new[] { "Tension (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherTensionHighlight", new[] { "Tension (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherStormcloudsBase", new[] { "Storm Clouds (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherStormcloudsHighlight", new[] { "Storm Clouds (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherRoughseasBase", new[] { "Rough Seas (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherRoughseasHighlight", new[] { "Rough Seas (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherLouringBase", new[] { "Louring (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherLouringHighlight", new[] { "Louring (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherEruptionsBase", new[] { "Eruptions (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherEruptionsHighlight", new[] { "Eruptions (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherIrradianceBase", new[] { "Irradiance (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherIrradianceHighlight", new[] { "Irradiance (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherCoreradiationBase", new[] { "Core Radiation (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherCoreradiationHighlight", new[] { "Core Radiation (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherShelfcloudsBase", new[] { "Shelf Clouds (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherShelfcloudsHighlight", new[] { "Shelf Clouds (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherOppressionBase", new[] { "Oppression (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherOppressionHighlight", new[] { "Oppression (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherUmbralwindBase", new[] { "Umbral Wind (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherUmbralwindHighlight", new[] { "Umbral Wind (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherUmbralstaticBase", new[] { "Umbral Static (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherUmbralstaticHighlight", new[] { "Umbral Static (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherSmokeBase", new[] { "Smoke (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherSmokeHighlight", new[] { "Smoke (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherRoyallevinBase", new[] { "Royal Levin (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherRoyallevinHighlight", new[] { "Royal Levin (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherHyperelectricityBase", new[] { "Hyperelectricity (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherHyperelectricityHighlight", new[] { "Hyperelectricity (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherMultiplicityBase", new[] { "Multiplicity (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherMultiplicityHighlight", new[] { "Multiplicity (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherDragonstormBase", new[] { "Dragonstorm (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherDragonstormHighlight", new[] { "Dragonstorm (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherSubterrainBase", new[] { "Subterrain (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherSubterrainHighlight", new[] { "Subterrain (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherConcordanceBase", new[] { "Concordance (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherConcordanceHighlight", new[] { "Concordance (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherBeyondtimeBase", new[] { "Beyond Time (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherBeyondtimeHighlight", new[] { "Beyond Time (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherDemonicinfinityBase", new[] { "Demonic Infinity (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherDemonicinfinityHighlight", new[] { "Demonic Infinity (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherDimensionaldisruptionBase", new[] { "Dimensional Disruption (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherDimensionaldisruptionHighlight", new[] { "Dimensional Disruption (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherRevelstormBase", new[] { "Revelstorm (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherRevelstormHighlight", new[] { "Revelstorm (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherEternalblissBase", new[] { "Eternal Bliss (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherEternalblissHighlight", new[] { "Eternal Bliss (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherWyrmstormBase", new[] { "Wyrmstorm (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherWyrmstormHighlight", new[] { "Wyrmstorm (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherQuicklevinBase", new[] { "Quicklevin (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherQuicklevinHighlight", new[] { "Quicklevin (Highlight)", "10", "Black", "White"}},
            {"ColorMappingWeatherWhitecycloneBase", new[] { "White Cyclone (Base)", "10", "Black", "White"}},
            {"ColorMappingWeatherWhitecycloneHighlight", new[] { "White Cyclone (Highlight)", "10", "Black", "White"}}
        };


        private readonly Dictionary<int, string> _paletteCategories = new Dictionary<int, string>
        {
            //Keys
            {0, "All"},
            {1, "Chromatics"},
            {2, "Player Stats"},
            {3, "Abilities"},
            {4, "Enmity/Aggro"},
            {5, "Target/Enemy"},
            {6, "Status Effects"},
            {7, "Cooldowns/Keybinds"},
            {8, "Notifications"},
            {9, "Job Gauges" },
            {10, "Reactive Weather" }
        };

        public void ResetDeviceDataGrid()
        {
            if (InvokeRequired)
            {
                ResetGridDelegate del = ResetDeviceDataGrid;
                Invoke(del);
            }
            else
            {
                SetupDeviceDataGrid();
            }
        }

        public void ResetMappingsDataGrid()
        {
            if (InvokeRequired)
            {
                ResetMappingsDelegate del = ResetMappingsDataGrid;
                Invoke(del);
            }
            else
            {
                SetupMappingsDataGrid();
            }
        }

        private void InitColorMappingGrid()
        {
            foreach (var c in _paletteCategories)
                cb_palette_categories.Items.Add(c.Value);

            cb_palette_categories.SelectedIndex = 0;
            ToggleMappingControls(false);

            ResetMappingsDataGrid();
        }

        private void ToggleMappingControls(bool toggle)
        {
            if (toggle)
            {
                mapping_colorEditorManager.ColorEditor.Enabled = true;
                mapping_colorEditorManager.ColorGrid.Enabled = true;
                mapping_colorEditorManager.ColorWheel.Enabled = true;
                mapping_colorEditorManager.LightnessColorSlider.Enabled = true;
                mapping_colorEditorManager.ScreenColorPicker.Enabled = true;
                loadPaletteButton.Enabled = true;
            }
            else
            {
                mapping_colorEditorManager.ColorEditor.Enabled = false;
                mapping_colorEditorManager.ColorGrid.Enabled = false;
                mapping_colorEditorManager.ColorWheel.Enabled = false;
                mapping_colorEditorManager.LightnessColorSlider.Enabled = false;
                mapping_colorEditorManager.ScreenColorPicker.Enabled = false;
                loadPaletteButton.Enabled = false;
            }
        }

        private void cb_palette_categories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MappingGridStartup)
            {
                var filter = cb_palette_categories.SelectedIndex.ToString();

                if (filter == "0")
                    foreach (DataGridViewRow row in dG_mappings.Rows)
                        row.Visible = true;
                else
                    foreach (DataGridViewRow row in dG_mappings.Rows)
                        if ((string)row.Cells[dG_mappings.Columns["mappings_col_cat"].Index].Value == filter)
                            row.Visible = true;
                        else
                            row.Visible = false;
            }
        }

        private void SetupMappingsDataGrid()
        {
            MappingGridStartup = false;
            dG_mappings.AllowUserToAddRows = true;
            dG_mappings.Rows.Clear();

            DrawMappingsDict();

            foreach (var palette in _mappingPalette)
            {
                var paletteItem = (DataGridViewRow)dG_mappings.Rows[0].Clone();
                paletteItem.Cells[dG_mappings.Columns["mappings_col_id"].Index].Value = palette.Key;
                paletteItem.Cells[dG_mappings.Columns["mappings_col_cat"].Index].Value = palette.Value[1];
                paletteItem.Cells[dG_mappings.Columns["mappings_col_type"].Index].Value = palette.Value[0];

                var paletteBtn = new DataGridViewTextBoxCell();
                paletteBtn.Style.BackColor = ColorTranslator.FromHtml(palette.Value[2]);
                paletteBtn.Style.SelectionBackColor = ColorTranslator.FromHtml(palette.Value[2]);

                paletteBtn.Value = "";
                paletteItem.Cells[dG_mappings.Columns["mappings_col_color"].Index] = paletteBtn;
                dG_mappings.Rows.Add(paletteItem);
                paletteBtn.ReadOnly = true;
            }

            dG_mappings.AllowUserToAddRows = false;
            MappingGridStartup = true;
        }

        private void DrawMappingsDict()
        {
            //PropertyInfo[] _FFXIVColorMappings = typeof(FFXIVColorMappings).GetProperties();

            //ColorMappings
            foreach (var p in typeof(FfxivColorMappings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var var = p.Name;
                var color = (string)p.GetValue(ColorMappings);
                var _data = _mappingPalette[var];
                string[] data = { _data[0], _data[1], color, _data[3] };
                _mappingPalette[var] = data;
            }
        }

        private void InitDeviceDataGrid()
        {
            //
        }

        private async void SetupDeviceDataGrid()
        {
            try
            {
                DeviceGridStartup = false;
                dG_devices.AllowUserToAddRows = true;
                dG_devices.Rows.Clear();

                /*
                if (RazerSdkCalled == 1)
                {
                    //Keyboard
                    var rzDgKeyboard = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    //DataGridViewRow _RzDgKeyboard = new DataGridViewRow();
                    rzDgKeyboard.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Razer Keyboard";
                    rzDgKeyboard.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Keyboard";
                    rzDgKeyboard.Cells[dG_devices.Columns["col_status"].Index].Value = _razerDeviceKeyboard
                        ? "Enabled"
                        : "Disabled";
                    rzDgKeyboard.Cells[dG_devices.Columns["col_state"].Index].Value = _razerDeviceKeyboard;
                    rzDgKeyboard.Cells[dG_devices.Columns["col_dattype"].Index].Value = "RazerDeviceKeyboard";

                    var rzDgKeyboardDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        rzDgKeyboardDgc.Items.Add(d.Value);

                    rzDgKeyboardDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    rzDgKeyboardDgc.DisplayStyleForCurrentCellOnly = true;
                    rzDgKeyboardDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                    rzDgKeyboard.Cells[dG_devices.Columns["col_mode"].Index] = rzDgKeyboardDgc;

                    dG_devices.Rows.Add(rzDgKeyboard);
                    rzDgKeyboardDgc.ReadOnly = true;

                    //Mouse
                    var rzDgMouse = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    rzDgMouse.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Razer Mouse";
                    rzDgMouse.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Mouse";
                    rzDgMouse.Cells[dG_devices.Columns["col_status"].Index].Value = _razerDeviceMouse
                        ? "Enabled"
                        : "Disabled";
                    rzDgMouse.Cells[dG_devices.Columns["col_state"].Index].Value = _razerDeviceMouse;
                    rzDgMouse.Cells[dG_devices.Columns["col_dattype"].Index].Value = "RazerDeviceMouse";

                    var rzDgMouseDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        rzDgMouseDgc.Items.Add(d.Value);

                    rzDgMouseDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    rzDgMouseDgc.DisplayStyleForCurrentCellOnly = true;
                    rzDgMouseDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    rzDgMouse.Cells[dG_devices.Columns["col_mode"].Index] = rzDgMouseDgc;

                    dG_devices.Rows.Add(rzDgMouse);
                    rzDgMouseDgc.ReadOnly = true;

                    //Headset
                    var rzDgHeadset = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    rzDgHeadset.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Razer Headset";
                    rzDgHeadset.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Headset";
                    rzDgHeadset.Cells[dG_devices.Columns["col_status"].Index].Value = _razerDeviceHeadset
                        ? "Enabled"
                        : "Disabled";
                    rzDgHeadset.Cells[dG_devices.Columns["col_state"].Index].Value = _razerDeviceHeadset;
                    rzDgHeadset.Cells[dG_devices.Columns["col_dattype"].Index].Value = "RazerDeviceHeadset";

                    var rzDgHeadsetDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        rzDgHeadsetDgc.Items.Add(d.Value);

                    rzDgHeadsetDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    rzDgHeadsetDgc.DisplayStyleForCurrentCellOnly = true;
                    rzDgHeadsetDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    rzDgHeadset.Cells[dG_devices.Columns["col_mode"].Index] = rzDgHeadsetDgc;

                    dG_devices.Rows.Add(rzDgHeadset);
                    rzDgHeadsetDgc.ReadOnly = true;

                    //Mousepad
                    var rzDgMousepad = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    rzDgMousepad.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Razer Mousepad";
                    rzDgMousepad.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Mousepad";
                    rzDgMousepad.Cells[dG_devices.Columns["col_status"].Index].Value = _razerDeviceMousepad
                        ? "Enabled"
                        : "Disabled";
                    rzDgMousepad.Cells[dG_devices.Columns["col_state"].Index].Value = _razerDeviceMousepad;
                    rzDgMousepad.Cells[dG_devices.Columns["col_dattype"].Index].Value = "RazerDeviceMousepad";

                    var rzDgMousepadDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        rzDgMousepadDgc.Items.Add(d.Value);

                    rzDgMousepadDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    rzDgMousepadDgc.DisplayStyleForCurrentCellOnly = true;
                    rzDgMousepadDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    rzDgMousepad.Cells[dG_devices.Columns["col_mode"].Index] = rzDgMousepadDgc;

                    dG_devices.Rows.Add(rzDgMousepad);
                    rzDgMousepadDgc.ReadOnly = true;

                    //Keypad
                    var rzDgKeypad = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    rzDgKeypad.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Razer Keypad";
                    rzDgKeypad.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Keypad";
                    rzDgKeypad.Cells[dG_devices.Columns["col_status"].Index].Value = _razerDeviceKeypad
                        ? "Enabled"
                        : "Disabled";
                    rzDgKeypad.Cells[dG_devices.Columns["col_state"].Index].Value = _razerDeviceKeypad;
                    rzDgKeypad.Cells[dG_devices.Columns["col_dattype"].Index].Value = "RazerDeviceKeypad";

                    var rzDgKeypadDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        rzDgKeypadDgc.Items.Add(d.Value);

                    rzDgKeypadDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    rzDgKeypadDgc.DisplayStyleForCurrentCellOnly = true;
                    rzDgKeypadDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    rzDgKeypad.Cells[dG_devices.Columns["col_mode"].Index] = rzDgKeypadDgc;

                    dG_devices.Rows.Add(rzDgKeypad);
                    rzDgKeypadDgc.ReadOnly = true;

                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                }

                if (CorsairSdkCalled == 1)
                {
                    //Keyboard
                    var corsairDgKeyboard = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    corsairDgKeyboard.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Corsair Keyboard";
                    corsairDgKeyboard.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Keyboard";
                    corsairDgKeyboard.Cells[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceKeyboard
                        ? "Enabled"
                        : "Disabled";
                    corsairDgKeyboard.Cells[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceKeyboard;
                    corsairDgKeyboard.Cells[dG_devices.Columns["col_dattype"].Index].Value = "CorsairDeviceKeyboard";

                    var corsairDgKeyboardDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        corsairDgKeyboardDgc.Items.Add(d.Value);

                    corsairDgKeyboardDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    corsairDgKeyboardDgc.DisplayStyleForCurrentCellOnly = true;
                    corsairDgKeyboardDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    corsairDgKeyboard.Cells[dG_devices.Columns["col_mode"].Index] = corsairDgKeyboardDgc;

                    dG_devices.Rows.Add(corsairDgKeyboard);
                    corsairDgKeyboardDgc.ReadOnly = true;

                    //Mouse
                    var corsairDgMouse = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    corsairDgMouse.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Corsair Mouse";
                    corsairDgMouse.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Mouse";
                    corsairDgMouse.Cells[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceMouse
                        ? "Enabled"
                        : "Disabled";
                    corsairDgMouse.Cells[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceMouse;
                    corsairDgMouse.Cells[dG_devices.Columns["col_dattype"].Index].Value = "CorsairDeviceMouse";

                    var corsairDgMouseDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        corsairDgMouseDgc.Items.Add(d.Value);

                    corsairDgMouseDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    corsairDgMouseDgc.DisplayStyleForCurrentCellOnly = true;
                    corsairDgMouseDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    corsairDgMouse.Cells[dG_devices.Columns["col_mode"].Index] = corsairDgMouseDgc;

                    dG_devices.Rows.Add(corsairDgMouse);
                    corsairDgMouseDgc.ReadOnly = true;

                    //Headset
                    var corsairDgHeadset = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    corsairDgHeadset.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Corsair Headset";
                    corsairDgHeadset.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Headset";
                    corsairDgHeadset.Cells[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceHeadset
                        ? "Enabled"
                        : "Disabled";
                    corsairDgHeadset.Cells[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceHeadset;
                    corsairDgHeadset.Cells[dG_devices.Columns["col_dattype"].Index].Value = "CorsairDeviceHeadset";

                    var corsairDgHeadsetDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        corsairDgHeadsetDgc.Items.Add(d.Value);

                    corsairDgHeadsetDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    corsairDgHeadsetDgc.DisplayStyleForCurrentCellOnly = true;
                    corsairDgHeadsetDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    corsairDgHeadset.Cells[dG_devices.Columns["col_mode"].Index] = corsairDgHeadsetDgc;

                    dG_devices.Rows.Add(corsairDgHeadset);
                    corsairDgHeadsetDgc.ReadOnly = true;

                    //Mousepad
                    var corsairDgMousepad = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    corsairDgMousepad.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Corsair Mousepad";
                    corsairDgMousepad.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Mousepad";
                    corsairDgMousepad.Cells[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceMousepad
                        ? "Enabled"
                        : "Disabled";
                    corsairDgMousepad.Cells[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceMousepad;
                    corsairDgMousepad.Cells[dG_devices.Columns["col_dattype"].Index].Value = "CorsairDeviceMousepad";

                    var corsairDgMousepadDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        corsairDgMousepadDgc.Items.Add(d.Value);

                    corsairDgMousepadDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    corsairDgMousepadDgc.DisplayStyleForCurrentCellOnly = true;
                    corsairDgMousepadDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    corsairDgMousepad.Cells[dG_devices.Columns["col_mode"].Index] = corsairDgMousepadDgc;

                    dG_devices.Rows.Add(corsairDgMousepad);
                    corsairDgMousepadDgc.ReadOnly = true;

                    _corsair.ResetCorsairDevices(_corsairDeviceKeyboard, _corsairDeviceKeypad, _corsairDeviceMouse,
                        _corsairDeviceMousepad, _corsairDeviceHeadset,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                }

                if (LogitechSdkCalled == 1)
                {
                    var logitechDgDevices = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    logitechDgDevices.Cells[dG_devices.Columns["col_devicename"].Index].Value = "Logitech Devices";
                    logitechDgDevices.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Multiple Devices";
                    logitechDgDevices.Cells[dG_devices.Columns["col_status"].Index].Value = _logitechDeviceKeyboard
                        ? "Enabled"
                        : "Disabled";
                    logitechDgDevices.Cells[dG_devices.Columns["col_state"].Index].Value = _logitechDeviceKeyboard;
                    logitechDgDevices.Cells[dG_devices.Columns["col_dattype"].Index].Value = "LogitechDevice";

                    var logitechDgDevicesDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        logitechDgDevicesDgc.Items.Add(d.Value);

                    logitechDgDevicesDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    logitechDgDevicesDgc.DisplayStyleForCurrentCellOnly = true;
                    logitechDgDevicesDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    logitechDgDevices.Cells[dG_devices.Columns["col_mode"].Index] = logitechDgDevicesDgc;

                    dG_devices.Rows.Add(logitechDgDevices);
                    logitechDgDevicesDgc.ReadOnly = true;

                    _logitech.ResetLogitechDevices(_logitechDeviceKeyboard,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                }

                if (CoolermasterSdkCalled == 1)
                {
                    var coolermasterDgDevices = (DataGridViewRow)dG_devices.Rows[0].Clone();
                    coolermasterDgDevices.Cells[dG_devices.Columns["col_devicename"].Index].Value =
                        "Coolermaster Devices";
                    coolermasterDgDevices.Cells[dG_devices.Columns["col_devicetype"].Index].Value = "Multiple Devices";
                    coolermasterDgDevices.Cells[dG_devices.Columns["col_status"].Index].Value =
                        _coolermasterDeviceKeyboard
                            ? "Enabled"
                            : "Disabled";
                    coolermasterDgDevices.Cells[dG_devices.Columns["col_state"].Index].Value =
                        _coolermasterDeviceKeyboard;
                    coolermasterDgDevices.Cells[dG_devices.Columns["col_dattype"].Index].Value = "CoolermasterDevice";

                    var coolermasterDgDevicesDgc = new DataGridViewComboBoxCell();
                    foreach (var d in _bulbModes)
                        coolermasterDgDevicesDgc.Items.Add(d.Value);

                    coolermasterDgDevicesDgc.Value = _bulbModes[BulbModeTypes.ChromaticsDefault];
                    coolermasterDgDevicesDgc.DisplayStyleForCurrentCellOnly = true;
                    coolermasterDgDevicesDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;

                    coolermasterDgDevices.Cells[dG_devices.Columns["col_mode"].Index] = coolermasterDgDevicesDgc;

                    dG_devices.Rows.Add(coolermasterDgDevices);
                    coolermasterDgDevicesDgc.ReadOnly = true;

                    _coolermaster.ResetCoolermasterDevices(_coolermasterDeviceKeyboard, _coolermasterDeviceMouse,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                }
                */
                if (LifxSdkCalled == 1)
                    if (_lifx.LifxBulbs > 0)
                        foreach (var d in _lifx.LifxBulbsDat.ToList())
                        {
                            Thread.Sleep(1);

                            //LIFX Device
                            var state = await _lifx.GetLightStateAsync(d.Key);
                            var device = await _lifx.GetDeviceVersionAsync(d.Key);

                            _dGmode.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
                            _dGmode.DisplayStyleForCurrentCellOnly = true;
                            var lState = _lifx.LifxStateMemory[d.Key.MacAddressName];

                            var devname = _lifx.LifXproductids[0];

                            if (_lifx.LifXproductids.ContainsKey(device.Product))
                            {
                                devname = _lifx.LifXproductids[device.Product];
                            }

                            var lifxdGDevice = (DataGridViewRow)dG_devices.Rows[0].Clone();
                            lifxdGDevice.Cells[dG_devices.Columns["col_devicename"].Index].Value = state.Label + " (" +
                                                                                                   d.Key
                                                                                                       .MacAddressName +
                                                                                                   ")";
                            lifxdGDevice.Cells[dG_devices.Columns["col_devicetype"].Index].Value =
                                devname + " (Version " + device.Version + ")";
                            lifxdGDevice.Cells[dG_devices.Columns["col_status"].Index].Value = lState == 0
                                ? "Disabled"
                                : "Enabled";
                            lifxdGDevice.Cells[dG_devices.Columns["col_state"].Index].Value = lState == 0
                                ? false
                                : true;
                            lifxdGDevice.Cells[dG_devices.Columns["col_dattype"].Index].Value = "LIFX";
                            lifxdGDevice.Cells[dG_devices.Columns["col_ID"].Index].Value = d.Key.MacAddressName;

                            var lifxdGDeviceDgc = new DataGridViewComboBoxCell();

                            foreach (var x in _bulbModes.ToList())
                            {
                                if (d.Value != 0 && x.Key == BulbModeTypes.Disabled) continue;
                                if (x.Key == BulbModeTypes.ChromaticsDefault) continue;
                                lifxdGDeviceDgc.Items.Add(x.Value);
                            }

                            lifxdGDeviceDgc.Value = d.Value == BulbModeTypes.ChromaticsDefault
                                ? _bulbModes[BulbModeTypes.Standby]
                                : _bulbModes[d.Value];
                            lifxdGDeviceDgc.DisplayStyleForCurrentCellOnly = true;
                            lifxdGDeviceDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

                            lifxdGDevice.Cells[dG_devices.Columns["col_mode"].Index] = lifxdGDeviceDgc;

                            //Check for duplicates

                            /*
                            var duplicate = false;

                            if (DeviceGridStartup)
                            {
                                foreach (DataGridViewRow row in dG_devices.Rows)
                                {
                                    if (row.Cells[dG_devices.Columns["col_ID"].Index].Value.ToString().Contains(d.Key.MacAddressName))
                                    {
                                        duplicate = true;
                                        break;
                                    }
                                    
                                }
                            }

                            if (duplicate) continue;
                            */

        dG_devices.Rows.Add(lifxdGDevice);
                            lifxdGDeviceDgc.ReadOnly = d.Value == 0 ? true : false;
                        }

                if (HueSdkCalled == 1)
                    if (_hue.HueBulbs > 0)
                        foreach (var d in _hue.HueBulbsDat.ToList())
                        {
                            Thread.Sleep(1);

                            //HUE Device
                            var state = await _hue.GetLightStateAsync(d.Key);
                            var device = await _hue.GetDeviceVersionAsync(d.Key);

                            _dGmode.DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox;
                            _dGmode.DisplayStyleForCurrentCellOnly = true;
                            var lState = _hue.HueStateMemory[d.Key.UniqueId];

                            var huedGDevice = (DataGridViewRow)dG_devices.Rows[0].Clone();
                            huedGDevice.Cells[dG_devices.Columns["col_devicename"].Index].Value = d.Key.Name + " (" +
                                                                                                  d.Key.UniqueId +
                                                                                                  ")";
                            huedGDevice.Cells[dG_devices.Columns["col_devicetype"].Index].Value =
                                d.Key.ModelId + " (Version " + device + ")";
                            huedGDevice.Cells[dG_devices.Columns["col_status"].Index].Value = lState == 0
                                ? "Disabled"
                                : "Enabled";
                            huedGDevice.Cells[dG_devices.Columns["col_state"].Index].Value = lState == 0
                                ? false
                                : true;
                            huedGDevice.Cells[dG_devices.Columns["col_dattype"].Index].Value = "HUE";
                            huedGDevice.Cells[dG_devices.Columns["col_ID"].Index].Value = d.Key.UniqueId;

                            var huedGDeviceDgc = new DataGridViewComboBoxCell();

                            foreach (var x in _bulbModes.ToList())
                            {
                                if (d.Value != 0 && x.Key == BulbModeTypes.Disabled) continue;
                                if (x.Key == BulbModeTypes.ChromaticsDefault) continue;
                                huedGDeviceDgc.Items.Add(x.Value);
                            }

                            huedGDeviceDgc.Value = d.Value == BulbModeTypes.ChromaticsDefault
                                ? _bulbModes[BulbModeTypes.Standby]
                                : _bulbModes[d.Value];
                            huedGDeviceDgc.DisplayStyleForCurrentCellOnly = true;
                            huedGDeviceDgc.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;

                            huedGDevice.Cells[dG_devices.Columns["col_mode"].Index] = huedGDeviceDgc;

                            //Check for duplicates
                            /*
                            var duplicate = false;

                            if (DeviceGridStartup)
                            {
                                foreach (DataGridViewRow row in dG_devices.Rows)
                                {
                                    if (row.Cells[dG_devices.Columns["col_ID"].Index].Value.ToString().Contains(d.Key.UniqueId))
                                    {
                                        duplicate = true;
                                        break;
                                    }

                                }
                            }

                            if (duplicate) continue;
                            */
                            dG_devices.Rows.Add(huedGDevice);
                            huedGDeviceDgc.ReadOnly = d.Value == 0 ? true : false;
                        }

                DeviceGridStartup = true;
                dG_devices.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                WriteConsole(ConsoleTypes.Error, "EX: " + ex.Message);
                WriteConsole(ConsoleTypes.Error, "EX: " + ex.StackTrace);
            }
        }

        private void dG_devices_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (DeviceGridStartup)
            {
                var editedCell = dG_devices.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var dattype = (string)dG_devices.Rows[e.RowIndex].Cells[dG_devices.Columns["col_dattype"].Index]
                    .Value;

                if (dG_devices.Columns[e.ColumnIndex].Name == "col_state")
                {
                    var _switch = (bool)editedCell.Value;
                    var modeX = (string)dG_devices.Rows[e.RowIndex].Cells[dG_devices.Columns["col_mode"].Index].Value;
                    var key = _bulbModes.FirstOrDefault(x => x.Value == modeX).Key;
                    var change = dG_devices.CurrentRow.Cells;

                    /*
                    if (dattype == "RazerDeviceKeyboard")
                    {
                        if (_switch)
                            _razerDeviceKeyboard = true;
                        else
                            _razerDeviceKeyboard = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _razerDeviceKeyboard
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _razerDeviceKeyboard;

                        if (RazerSdkCalled == 1)
                            _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                                _razerDeviceMousepad, _razerDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "RazerDeviceMouse")
                    {
                        if (_switch)
                            _razerDeviceMouse = true;
                        else
                            _razerDeviceMouse = false;

                        change[dG_devices.Columns["col_status"].Index].Value =
                            _razerDeviceMouse ? "Enabled" : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _razerDeviceMouse;

                        if (RazerSdkCalled == 1)
                            _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                                _razerDeviceMousepad, _razerDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "RazerDeviceHeadset")
                    {
                        if (_switch)
                            _razerDeviceHeadset = true;
                        else
                            _razerDeviceHeadset = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _razerDeviceHeadset
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _razerDeviceHeadset;

                        if (RazerSdkCalled == 1)
                            _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                                _razerDeviceMousepad, _razerDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "RazerDeviceMousepad")
                    {
                        if (_switch)
                            _razerDeviceMousepad = true;
                        else
                            _razerDeviceMousepad = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _razerDeviceMousepad
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _razerDeviceMousepad;

                        if (RazerSdkCalled == 1)
                            _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                                _razerDeviceMousepad, _razerDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "RazerDeviceKeypad")
                    {
                        if (_switch)
                            _razerDeviceKeypad = true;
                        else
                            _razerDeviceKeypad = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _razerDeviceKeypad
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _razerDeviceKeypad;

                        if (RazerSdkCalled == 1)
                            _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                                _razerDeviceMousepad, _razerDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "CorsairDeviceKeyboard")
                    {
                        if (_switch)
                            _corsairDeviceKeyboard = true;
                        else
                            _corsairDeviceKeyboard = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceKeyboard
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceKeyboard;

                        if (CorsairSdkCalled == 1)
                            _corsair.ResetCorsairDevices(_corsairDeviceKeyboard, _corsairDeviceKeypad, _corsairDeviceMouse,
                                _corsairDeviceMousepad, _corsairDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "CorsairDeviceMouse")
                    {
                        if (_switch)
                            _corsairDeviceMouse = true;
                        else
                            _corsairDeviceMouse = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceMouse
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceMouse;

                        if (CorsairSdkCalled == 1)
                            _corsair.ResetCorsairDevices(_corsairDeviceKeyboard, _corsairDeviceKeypad, _corsairDeviceMouse,
                                _corsairDeviceMousepad, _corsairDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "CorsairDeviceHeadset")
                    {
                        if (_switch)
                            _corsairDeviceHeadset = true;
                        else
                            _corsairDeviceHeadset = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceHeadset
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceHeadset;

                        if (CorsairSdkCalled == 1)
                            _corsair.ResetCorsairDevices(_corsairDeviceKeyboard, _corsairDeviceKeypad, _corsairDeviceMouse,
                                _corsairDeviceMousepad, _corsairDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "CorsairDeviceMousepad")
                    {
                        if (_switch)
                            _corsairDeviceMousepad = true;
                        else
                            _corsairDeviceMousepad = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _corsairDeviceMousepad
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _corsairDeviceMousepad;

                        if (CorsairSdkCalled == 1)
                            _corsair.ResetCorsairDevices(_corsairDeviceKeyboard, _corsairDeviceKeypad, _corsairDeviceMouse,
                                _corsairDeviceMousepad, _corsairDeviceHeadset,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "LogitechDevice")
                    {
                        if (_switch)
                            _logitechDeviceKeyboard = true;
                        else
                            _logitechDeviceKeyboard = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _logitechDeviceKeyboard
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _logitechDeviceKeyboard;

                        if (LogitechSdkCalled == 1)
                            _logitech.ResetLogitechDevices(_logitechDeviceKeyboard,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    else if (dattype == "CoolermasterDevice")
                    {
                        if (_switch)
                            _coolermasterDeviceKeyboard = true;
                        else
                            _coolermasterDeviceKeyboard = false;

                        change[dG_devices.Columns["col_status"].Index].Value = _coolermasterDeviceKeyboard
                            ? "Enabled"
                            : "Disabled";
                        change[dG_devices.Columns["col_state"].Index].Value = _coolermasterDeviceKeyboard;

                        if (CoolermasterSdkCalled == 1)
                            _coolermaster.ResetCoolermasterDevices(_coolermasterDeviceKeyboard, _coolermasterDeviceMouse,
                                ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                    }
                    */
                    if (dattype == "LIFX")
                    {
                        var id = (string)dG_devices.Rows[e.RowIndex].Cells[dG_devices.Columns["col_ID"].Index].Value;
                        if (LifxSdkCalled == 1 && id != null)
                        {
                            var bulb = _lifx.LifxDevices[id];

                            if (_switch)
                            {
                                _lifx.LifxBulbsDat[bulb] = _lifx.LifxModeMemory[id];
                                _lifx.LifxStateMemory[bulb.MacAddressName] = 1;
                                WriteConsole(ConsoleTypes.Lifx, "Enabled LIFX Bulb " + id);
                            }
                            else
                            {
                                _lifx.LifxModeMemory[id] = key;
                                _lifx.LifxStateMemory[bulb.MacAddressName] = 0;
                                _lifx.LifxBulbsDat[bulb] = 0;
                                var state = _lifx.LifxBulbsRestore[bulb];
                                WriteConsole(ConsoleTypes.Lifx, "Disabled LIFX Bulb " + id);
                                _lifx.SetColorAsync(bulb, state.Hue, state.Saturation, state.Brightness, state.Kelvin,
                                    TimeSpan.FromMilliseconds(1000));
                                WriteConsole(ConsoleTypes.Lifx, "Restoring LIFX Bulb " + state.Label);
                            }

                            change[dG_devices.Columns["col_status"].Index].Value = _switch ? "Enabled" : "Disabled";
                        }
                    }
                    else if (dattype == "HUE")
                    {
                        var id = (string)dG_devices.Rows[e.RowIndex].Cells[dG_devices.Columns["col_ID"].Index].Value;
                        if (HueSdkCalled == 1 && id != null)
                        {
                            var bulb = _hue.HueDevices[id];

                            if (_switch)
                            {
                                _hue.HueBulbsDat[bulb] = _hue.HueModeMemory[id];
                                _hue.HueStateMemory[bulb.UniqueId] = 1;
                                WriteConsole(ConsoleTypes.Hue, "Enabled HUE Bulb " + id);
                            }
                            else
                            {
                                _hue.HueModeMemory[id] = key;
                                _hue.HueStateMemory[bulb.UniqueId] = 0;
                                _hue.HueBulbsDat[bulb] = 0;
                                var state = _hue.HueBulbsRestore[bulb];
                                WriteConsole(ConsoleTypes.Hue, "Disabled HUE Bulb " + id);

                                _hue.SetColorAsync(bulb, state.Hue, state.Saturation, state.Brightness,
                                    state.ColorTemperature,
                                    TimeSpan.FromMilliseconds(1000));

                                WriteConsole(ConsoleTypes.Hue, "Restoring HUE Bulb " + bulb.Name);
                            }

                            change[dG_devices.Columns["col_status"].Index].Value = _switch ? "Enabled" : "Disabled";
                        }
                    }
                }

                if (dG_devices.Columns[e.ColumnIndex].Name == "col_mode")
                {
                    var mode = (string)editedCell.Value;
                    var id = (string)dG_devices.Rows[e.RowIndex].Cells[dG_devices.Columns["col_ID"].Index].Value;

                    if (LifxSdkCalled == 1 && id != null && dattype == "LIFX")
                        if (_bulbModes.ContainsValue(mode))
                        {
                            var key = _bulbModes.FirstOrDefault(x => x.Value == mode).Key;
                            var bulb = _lifx.LifxDevices[id];
                            _lifx.LifxBulbsDat[bulb] = key;
                            _lifx.LifxModeMemory[bulb.MacAddressName] = key;
                            WriteConsole(ConsoleTypes.Lifx, "Updated Mode of LIFX Bulb " + id + " to " + key);
                        }

                    if (HueSdkCalled == 1 && id != null && dattype == "HUE")
                        if (_bulbModes.ContainsValue(mode))
                        {
                            var key = _bulbModes.FirstOrDefault(x => x.Value == mode).Key;
                            var bulb = _hue.HueDevices[id];
                            _hue.HueBulbsDat[bulb] = key;
                            _hue.HueModeMemory[bulb.UniqueId] = key;
                            WriteConsole(ConsoleTypes.Lifx, "Updated Mode of HUE Bulb " + id + " to " + key);
                        }
                }

                SaveDevices();
                //ResetDeviceDataGrid();

                /*
                if (RazerSdkCalled == 1)
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                */
            }
        }

        private void CenterPictureBox(PictureBox picBox, Image picImage)
        {
            /*
            picBox.Image = picImage;
            picBox.Location = new Point((picBox.Parent.ClientSize.Width / 2) - (picImage.Width / 2),
                                        (picBox.Parent.ClientSize.Height / 2) - (picImage.Height / 2));
            picBox.Refresh();
            */
            var xpos = picBox.Parent.Width / 2 - picImage.Width / 2;
            var ypos = picBox.Parent.Height / 2 - picImage.Height / 2;
            picBox.Location = new Point(xpos, ypos);
        }

        private void dG_mappings_SelectionChanged(object sender, EventArgs e)
        {
            var color = dG_mappings.CurrentRow.Cells[dG_mappings.Columns["mappings_col_color"].Index].Style.BackColor;
            ToggleMappingControls(true);
            mapping_colorEditorManager.Color = color;
            previewPanel.BackColor = color;
            //PaletteMappingCurrentSelect = dG_mappings.CurrentRow.Index;
        }

        private void dG_mappings_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            e.PaintParts &= ~DataGridViewPaintParts.Focus;
        }

        private void mapping_colorEditorManager_ColorChanged(object sender, EventArgs e)
        {
            var pmcs = dG_mappings.CurrentRow;
            var pcmsid = (string)pmcs.Cells[dG_mappings.Columns["mappings_col_id"].Index].Value;
            var pcmsColor = pmcs.Cells[dG_mappings.Columns["mappings_col_color"].Index].Style.BackColor;

            previewPanel.BackColor = mapping_colorEditorManager.Color;

            if (pcmsColor != mapping_colorEditorManager.Color)
            {
                var _data = _mappingPalette[pcmsid];
                string[] data = { _data[0], _data[1], _data[2], ColorTranslator.ToHtml(pcmsColor) };
                _mappingPalette[pcmsid] = data;

                foreach (var p in typeof(FfxivColorMappings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    if (p.Name == pcmsid)
                        p.SetValue(ColorMappings, ColorTranslator.ToHtml(mapping_colorEditorManager.Color));

                DrawMappingsDict();
                pmcs.Cells[dG_mappings.Columns["mappings_col_color"].Index].Style.BackColor =
                    mapping_colorEditorManager.Color;
                pmcs.Cells[dG_mappings.Columns["mappings_col_color"].Index].Style.SelectionBackColor =
                    mapping_colorEditorManager.Color;

                SetKeysbase = false;
                SetMousebase = false;
                SetPadbase = false;
                SaveColorMappings(0);
            }
        }

        private void btn_palette_undo_Click(object sender, EventArgs e)
        {
            var pmcs = dG_mappings.CurrentRow;
            var pcmsid = (string)pmcs.Cells[dG_mappings.Columns["mappings_col_id"].Index].Value;

            var cm = new FfxivColorMappings();
            var _restore = ColorTranslator.ToHtml(Color.Black);

            foreach (var p in typeof(FfxivColorMappings).GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (p.Name == pcmsid)
                    _restore = (string)p.GetValue(cm);

            var restore = ColorTranslator.FromHtml(_restore);
            mapping_colorEditorManager.Color = restore;
        }

        private void loadPaletteButton_Click(object sender, EventArgs e)
        {
            using (FileDialog dialog = new OpenFileDialog
            {
                Filter = PaletteSerializer.DefaultOpenFilter,
                DefaultExt = "pal",
                Title = "Open Palette File"
            })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                    try
                    {
                        IPaletteSerializer serializer;

                        serializer = PaletteSerializer.GetSerializer(dialog.FileName);
                        if (serializer != null)
                        {
                            ColorCollection palette;

                            if (!serializer.CanRead)
                                throw new InvalidOperationException("Serializer does not support reading palettes.");

                            using (var file = File.OpenRead(dialog.FileName))
                            {
                                palette = serializer.Deserialize(file);
                            }

                            if (palette != null)
                            {
                                // we can only display 96 colors in the color grid due to it's size, so if there's more, bin them
                                while (palette.Count > 96)
                                    palette.RemoveAt(palette.Count - 1);

                                // or if we have less, fill in the blanks
                                while (palette.Count < 96)
                                    palette.Add(Color.White);

                                colorGrid1.Colors = palette;
                            }
                        }
                        else
                        {
                            MessageBox.Show(
                                @"Sorry, unable to open palette, the file format is not supported or is not recognized.",
                                @"Load Palette", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            string.Format("Sorry, unable to open palette. {0}", ex.GetBaseException().Message),
                            @"Load Palette", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
            }
        }

        private void btn_importChromatics_Click(object sender, EventArgs e)
        {
            ImportColorMappings();
        }

        private void btn_exportChromatics_Click(object sender, EventArgs e)
        {
            ExportColorMappings();
        }

        private void SetupTooltips()
        {
            var ttBtnImport = new ToolTip();
            ttBtnImport.SetToolTip(btn_importChromatics, "Import Chromatics Color Palette");

            var ttBtnExport = new ToolTip();
            ttBtnExport.SetToolTip(btn_exportChromatics, "Export Chromatics Color Palette");

            var ttBtnLoadpalette = new ToolTip();
            ttBtnLoadpalette.SetToolTip(loadPaletteButton, "Import External Color Swatches");

            var ttBtnPaletteUndo = new ToolTip();
            ttBtnPaletteUndo.SetToolTip(btn_palette_undo, "Restore to Default");
        }

        private void InitSettingsGui()
        {
            chk_arxtoggle.Checked = ChromaticsSettings.ChromaticsSettingsArxToggle;
            cb_arx_theme.SelectedIndex = ChromaticsSettings.ChromaticsSettingsArxTheme;
            mi_arxenable.Checked = ChromaticsSettings.ChromaticsSettingsArxToggle;
            chk_lccauto.Checked = ChromaticsSettings.ChromaticsSettingsLccAuto;
            chk_memorycache.Checked = ChromaticsSettings.ChromaticsSettingsMemoryCache;
            //chk_azertymode.Checked = ChromaticsSettings.ChromaticsSettingsAzertyMode;
            chk_desktopnotify.Checked = ChromaticsSettings.ChromaticsSettingsDesktopNotifications;

            cb_qwerty.SelectedIndex = (int)ChromaticsSettings.ChromaticsSettingsQwertyMode;

            chk_castanimatetoggle.Checked = ChromaticsSettings.ChromaticsSettingsCastAnimate;
            chk_castchargetoggle.Checked = ChromaticsSettings.ChromaticsSettingsCastToggle;
            chk_gcdcounttoggle.Checked = ChromaticsSettings.ChromaticsSettingsGcdCountdown;

            chk_keybindtoggle.Checked = ChromaticsSettings.ChromaticsSettingsKeybindToggle;
            chk_jobgaugetoggle.Checked = ChromaticsSettings.ChromaticsSettingsJobGaugeToggle;
            chk_highlighttoggle.Checked = ChromaticsSettings.ChromaticsSettingsKeyHighlights;
            chk_impactflashtog.Checked = ChromaticsSettings.ChromaticsSettingsImpactToggle;
            chk_dfbelltoggle.Checked = ChromaticsSettings.ChromaticsSettingsDfBellToggle;
            chk_showstats.Checked = ChromaticsSettings.ChromaticsSettingsShowStats;

            chk_lcdtoggle.Checked = ChromaticsSettings.ChromaticsSettingsLcdEnabled;
            cb_lang.SelectedIndex = ChromaticsSettings.ChromaticsSettingsLanguage;

            chk_reactiveweather.Checked = ChromaticsSettings.ChromaticsSettingsReactiveWeather;
            chk_debugopt.Checked = ChromaticsSettings.ChromaticsSettingsDebugOpt;

            chk_dev_keyboard.Checked = _deviceKeyboard;
            chk_dev_mouse.Checked = _deviceMouse;
            chk_dev_mousepad.Checked = _deviceMousepad;
            chk_dev_headset.Checked = _deviceHeadset;
            chk_dev_keypad.Checked = _deviceKeypad;
            chk_dev_chromalink.Checked = _deviceCL;

            foreach (var item in _devModesA)
            {
                cb_singlezonemode.Items.Add(item.Value);
                cb_mouse_z1.Items.Add(item.Value);
                cb_mouse_z2.Items.Add(item.Value);
                cb_mouse_z3.Items.Add(item.Value);
                cb_mouse_zs1.Items.Add(item.Value);
                cb_mouse_zs2.Items.Add(item.Value);
                cb_pad_zs1.Items.Add(item.Value);
                cb_pad_zs2.Items.Add(item.Value);
                cb_pad_zs3.Items.Add(item.Value);
                cb_headset_z1.Items.Add(item.Value);
                cb_keypad_z1.Items.Add(item.Value);
                cb_chromalink_z1.Items.Add(item.Value);
                cb_chromalink_z2.Items.Add(item.Value);
                cb_chromalink_z3.Items.Add(item.Value);
                cb_chromalink_z4.Items.Add(item.Value);
                cb_chromalink_z5.Items.Add(item.Value);
            }

            foreach (var item in _lightbarModes)
            {
                cb_lightbarmode.Items.Add(item.Value);
            }

            cb_singlezonemode.SelectedItem = Helpers.ConvertDevModeToCB(_KeysSingleKeyMode);
            cb_lightbarmode.SelectedItem = Helpers.ConvertLightbarModeToCB(_LightbarMode);

            cb_mouse_z1.SelectedItem = Helpers.ConvertDevModeToCB(_MouseZone1Mode);
            cb_mouse_z2.SelectedItem = Helpers.ConvertDevModeToCB(_MouseZone2Mode);
            cb_mouse_z3.SelectedItem = Helpers.ConvertDevModeToCB(_MouseZone3Mode);
            cb_mouse_zs1.SelectedItem = Helpers.ConvertDevModeToCB(_MouseStrip1Mode);
            cb_mouse_zs2.SelectedItem = Helpers.ConvertDevModeToCB(_MouseStrip2Mode);
            cb_pad_zs1.SelectedItem = Helpers.ConvertDevModeToCB(_PadZone1Mode);
            cb_pad_zs2.SelectedItem = Helpers.ConvertDevModeToCB(_PadZone2Mode);
            cb_pad_zs3.SelectedItem = Helpers.ConvertDevModeToCB(_PadZone3Mode);
            cb_headset_z1.SelectedItem = Helpers.ConvertDevModeToCB(_HeadsetZone1Mode);
            cb_keypad_z1.SelectedItem = Helpers.ConvertDevModeToCB(_KeypadZone1Mode);
            cb_chromalink_z1.SelectedItem = Helpers.ConvertDevModeToCB(_CLZone1Mode);
            cb_chromalink_z2.SelectedItem = Helpers.ConvertDevModeToCB(_CLZone2Mode);
            cb_chromalink_z3.SelectedItem = Helpers.ConvertDevModeToCB(_CLZone3Mode);
            cb_chromalink_z4.SelectedItem = Helpers.ConvertDevModeToCB(_CLZone4Mode);
            cb_chromalink_z5.SelectedItem = Helpers.ConvertDevModeToCB(_CLZone5Mode);

            chk_keys_singlemode.Checked = _KeysSingleKeyModeEnabled;
            cb_singlezonemode.Enabled = _KeysSingleKeyModeEnabled;
        }

        private void InitDevicesGui()
        {
            if (RazerSdkCalled == 1)
            {
                _razerDeviceKeyboard = _deviceKeyboard;
                _razerDeviceMouse = _deviceMouse;
                _razerDeviceMousepad = _deviceMousepad;
                _razerDeviceKeypad = _deviceKeypad;
                _razerDeviceHeadset = _deviceHeadset;
                _razerDeviceChromaLink = _deviceCL;

                /*
                gP_ChromaLink.Enabled = true;
                chk_dev_chromalink.Enabled = true;
                cb_chromalink_z1.Enabled = true;
                cb_chromalink_z2.Enabled = true;
                cb_chromalink_z3.Enabled = true;
                cb_chromalink_z4.Enabled = true;
                cb_chromalink_z5.Enabled = true;
                */
            }
            else
            {
                _razerDeviceKeyboard = false;
                _razerDeviceMouse = false;
                _razerDeviceMousepad = false;
                _razerDeviceKeypad = false;
                _razerDeviceHeadset = false;
                _razerDeviceChromaLink = false;

                /*
                chk_dev_chromalink.Enabled = false;
                cb_chromalink_z1.Enabled = false;
                cb_chromalink_z2.Enabled = false;
                cb_chromalink_z3.Enabled = false;
                cb_chromalink_z4.Enabled = false;
                cb_chromalink_z5.Enabled = false;
                gP_ChromaLink.Enabled = false;
                */
            }

            if (LogitechSdkCalled == 1)
            {
                _logitechDeviceKeyboard = _deviceKeyboard;
                //_logitechDeviceMouse = true;
                //_logitechDeviceMousepad = true;
                //_logitechDeviceKeypad = true;
                //_logitechDeviceHeadset = true;
            }
            else
            {
                _logitechDeviceKeyboard = false;
                //_logitechDeviceMouse = false;
                //_logitechDeviceMousepad = false;
                //_logitechDeviceKeypad = false;
                //_logitechDeviceHeadset = false;

                chk_lcdtoggle.Enabled = false;
            }

            if (CorsairSdkCalled == 1)
            {
                _corsairDeviceKeyboard = _deviceKeyboard;
                _corsairDeviceMouse = _deviceMouse;
                _corsairDeviceMousepad = _deviceMousepad;
                _corsairDeviceKeypad = _deviceKeypad;
                _corsairDeviceHeadset = _deviceHeadset;
            }
            else
            {
                _corsairDeviceKeyboard = false;
                _corsairDeviceMouse = false;
                _corsairDeviceMousepad = false;
                _corsairDeviceKeypad = false;
                _corsairDeviceHeadset = false;
            }

            if (CoolermasterSdkCalled == 1)
            {
                _coolermasterDeviceKeyboard = _deviceKeyboard;
                _coolermasterDeviceMouse = _deviceMouse;
                //_coolermasterDeviceMousepad = true;
                //_coolermasterDeviceKeypad = true;
                //_coolermasterDeviceHeadset = true;
            }
            else
            {
                _coolermasterDeviceKeyboard = false;
                _coolermasterDeviceMouse = false;
                //_coolermasterDeviceMousepad = false;
                //_coolermasterDeviceKeypad = false;
                //_coolermasterDeviceHeadset = false;
            }

            if (RoccatSdkCalled == 1)
            {
                _roccatDeviceKeyboard = _deviceKeyboard;
                _roccatDeviceMouse = _deviceMouse;
                //_roccatDeviceMousepad = true;
                //_roccatDeviceKeypad = true;
                //_roccatDeviceHeadset = true;
            }
            else
            {
                _roccatDeviceKeyboard = false;
                _roccatDeviceMouse = false;
                //_roccatDeviceMousepad = false;
                //_roccatDeviceKeypad = false;
                //_roccatDeviceHeadset = false;
            }

            GlobalResetDevices();
        }

        private void InitSettingsArxGui()
        {
            if (cb_arx_mode.Items.Contains(ChromaticsSettings.ChromaticsSettingsArxMode))
            {
                cb_arx_mode.SelectedItem = ChromaticsSettings.ChromaticsSettingsArxMode;
            }
            else
            {
                var chromaticsSettings = new ChromaticsSettings();
                cb_arx_mode.SelectedItem = chromaticsSettings.ChromaticsSettingsArxMode;
            }

            txt_arx_actip.Text = ChromaticsSettings.ChromaticsSettingsArxactip;
        }

        private void chk_arxtoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsArxToggle = chk_arxtoggle.Checked;
            SaveChromaticsSettings(1);

            if (chk_arxtoggle.Checked)
            {
                ArxToggle = true;
                cb_arx_theme.Enabled = true;
                cb_arx_mode.Enabled = true;

                if (ArxSdkCalled == 0)
                {
                    _arx = LogitechArxInterface.InitializeArxSdk();
                    if (_arx != null)
                    {
                        ArxSdk = true;
                        ArxSdkCalled = 1;
                        ArxState = 0;

                        WriteConsole(ConsoleTypes.Arx, "ARX SDK Enabled");

                        LoadArxPlugins();

                        var theme = "light";
                        var themeid = cb_arx_theme.SelectedIndex;

                        switch (themeid)
                        {
                            case 0:
                                theme = "light";
                                break;
                            case 1:
                                theme = "dark";
                                break;
                            case 2:
                                theme = "grey";
                                break;
                            case 3:
                                theme = "black";
                                break;
                            case 4:
                                theme = "cycle";
                                break;
                        }

                        _arx.ArxUpdateTheme(theme);
                    }
                }
            }
            else
            {
                ArxToggle = false;
                cb_arx_theme.Enabled = false;
                cb_arx_mode.Enabled = false;

                if (Plugs.Count > 0)
                    foreach (var plugin in Plugs)
                        if (cb_arx_mode.Items.Contains(plugin))
                            cb_arx_mode.Items.Remove(plugin);

                if (ArxSdkCalled == 1)
                {
                    _arx.ShutdownArx();
                    ArxSdkCalled = 0;
                    WriteConsole(ConsoleTypes.Arx, "ARX SDK Disabled");
                }
            }
        }

        private void cb_arx_theme_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ArxSdkCalled != 1) return;
            if (Startup == false) return;
            var theme = "light";
            var themeid = cb_arx_theme.SelectedIndex;
            ChromaticsSettings.ChromaticsSettingsArxTheme = cb_arx_theme.SelectedIndex;
            SaveChromaticsSettings(1);

            switch (themeid)
            {
                case 0:
                    theme = "light";
                    break;
                case 1:
                    theme = "dark";
                    break;
                case 2:
                    theme = "grey";
                    break;
                case 3:
                    theme = "black";
                    break;
                case 4:
                    theme = "cycle";
                    break;
            }

            _arx.ArxUpdateTheme(theme);
        }

        private void cb_arx_mode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ArxSdkCalled != 1) return;
            if (Startup == false) return;
            
            if (cb_arx_mode.SelectedIndex < 3)
            {
                ArxState = cb_arx_mode.SelectedIndex + 1;
                _arx.SetArxCurrentID(ArxState);

                switch (ArxState)
                {
                    case 1:
                        _arx.ArxSetIndex("playerhud.html");
                        break;
                    case 2:
                        _arx.ArxSetIndex("partylist.html");
                        break;
                    case 3:
                        _arx.ArxSetIndex("act.html");

                        var changed = txt_arx_actip.Text;
                        if (changed.EndsWith("/"))
                        {
                            changed = changed.Substring(0, changed.Length - 1);
                        }

                        _arx.ArxSendActInfo(changed, 8085);
                        break;
                }
            }
            else
            {
                var getPlugin = cb_arx_mode.SelectedItem + ".html";
                ArxState = 100;
                _arx.ArxSetIndex(getPlugin);
            }
            

            WriteConsole(ConsoleTypes.Arx, "ARX Template Changed: " + cb_arx_mode.SelectedItem);

            ChromaticsSettings.ChromaticsSettingsArxMode = cb_arx_mode.SelectedItem.ToString();
            SaveChromaticsSettings(1);
        }

        private void rtb_debug_TextChanged(object sender, EventArgs e)
        {
            rtb_debug.SelectionStart = rtb_debug.Text.Length;
            rtb_debug.ScrollToCaret();
        }

        private void showwindow_Click(object sender, EventArgs e)
        {
            if (!_allowVisible)
            {
                _allowVisible = true;
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void mi_updatecheck_Click(object sender, EventArgs e)
        {
            CheckUpdates(1);
        }

        private void mi_winstart_Click(object sender, EventArgs e)
        {
            if (mi_winstart.Checked)
                _rkApp.SetValue("Chromatics", Application.ExecutablePath);
            else
                _rkApp.DeleteValue("Chromatics", false);

            chk_startupenable.Checked = mi_winstart.Checked;
        }

        private void enableeffects_Click(object sender, EventArgs e)
        {
            //Enable effects
            if (!mi_effectsenable.Checked)
            {
                if (RazerSdk)
                    RazerSdkCalled = 0;
                if (LogitechSdk)
                    LogitechSdkCalled = 0;
                if (CorsairSdk)
                    CorsairSdkCalled = 0;
                if (LifxSdk)
                    LifxSdkCalled = 0;
                if (HueSdk)
                    HueSdkCalled = 0;
            }
            else
            {
                if (RazerSdk)
                    RazerSdkCalled = 1;
                if (LogitechSdk)
                    LogitechSdkCalled = 1;
                if (CorsairSdk)
                    CorsairSdkCalled = 1;
                if (LifxSdk)
                    LifxSdkCalled = 1;
                if (HueSdk)
                    HueSdkCalled = 1;
            }

            ResetDeviceDataGrid();
        }

        private void enablearx_Click(object sender, EventArgs e)
        {
            if (mi_arxenable.Checked)
                chk_arxtoggle.Checked = true;
            else
                chk_arxtoggle.Checked = false;
        }

        private void notify_master_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (!_allowVisible)
            {
                _allowVisible = true;
                Show();
                WindowState = FormWindowState.Normal;
            }
        }

        private void txt_arx_actip_TextChanged(object sender, EventArgs e)
        {
            if (Startup)
            {
                var changed = txt_arx_actip.Text;
                if (changed.EndsWith("/"))
                    changed = changed.Substring(0, changed.Length - 1);

                txt_arx_actip.Text = changed;
                _arx.ArxSendActInfo(changed, 8085);

                ChromaticsSettings.ChromaticsSettingsArxactip = txt_arx_actip.Text;
                SaveChromaticsSettings(1);
            }
        }

        private void chk_lccenable_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (chk_lccenable.Checked)
                ToggleLccMode(true);
            else
                ToggleLccMode(false);
        }

        private void chk_lccauto_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsLccAuto = chk_lccauto.Checked;
            SaveChromaticsSettings(1);
        }

        private void btn_lccrestore_Click(object sender, EventArgs e)
        {
            if (LogitechSdkCalled == 0)
                return;

            var lccrestoreCheck =
                MessageBox.Show(
                    @"Are you sure you wish to restore LGS to its default settings? This should only be done as a last resort.",
                    @"Restore LGS Settings to Default", MessageBoxButtons.OKCancel);
            if (lccrestoreCheck == DialogResult.OK)
                try
                {
                    while (Process.GetProcessesByName("ffxiv_dx11").Length > 0)
                    {
                        var lccrestoreWarning =
                            MessageBox.Show(@"You must close Final Fantasy XIV before using restore.",
                                @"Please close Final Fantasy XIV", MessageBoxButtons.RetryCancel);
                        if (lccrestoreWarning == DialogResult.Cancel)
                            return;
                    }

                    if (File.Exists(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll"))
                        File.Delete(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll");

                    if (File.Exists(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll.disabled"))
                        File.Delete(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll.disabled");

                    var enviroment = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
                    var path = enviroment + @"/LogitechLed.dll";

                    File.Copy(path, _lgsInstall + @"\SDK\LED\x64\LogitechLed.dll", true);
                    WriteConsole(ConsoleTypes.Logitech, "LGS has been restored to its default settings.");

                    chk_lccenable.CheckedChanged -= chk_lccenable_CheckedChanged;
                    chk_lccenable.Checked = false;
                    LCCStatus = false;
                    chk_lccenable.CheckedChanged += chk_lccenable_CheckedChanged;
                }
                catch (Exception ex)
                {
                    WriteConsole(ConsoleTypes.Error,
                        "An Error occurred trying to enable Logitech Conflict Mode. Error: " + ex.Message);
                }
            else if (lccrestoreCheck == DialogResult.Cancel)
                return;
        }

        private void ToggleLccMode([Optional] bool force, [Optional] bool antilog)
        {
            if (LogitechSdkCalled == 0)
                return;

            var _force = false;

            if (!force)
                _force = true;


            if ((!LCCStatus || force) && !_force)
            {
                //Enable LCC

                if (File.Exists(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll"))
                {
                    try
                    {
                        //File.Copy(LgsInstall + @"\SDK\LED\x64\LogitechLed.dll", LgsInstall + @"\SDK\LED\x64\LogitechLed.dll.disabled", true);
                        //File.Delete(LgsInstall + @"\SDK\LED\x64\LogitechLed.dll");
                        FileSystem.RenameFile(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll", "LogitechLed.dll.disabled");
                        LCCStatus = true;
                    }
                    catch (Exception ex)
                    {
                        if (!antilog)
                            WriteConsole(ConsoleTypes.Error,
                                "An Error occurred trying to enable Logitech Conflict Mode. Error: " + ex.Message);
                        return;
                    }

                    if (!antilog)
                        WriteConsole(ConsoleTypes.Logitech, "Logitech Conflict Mode Enabled.");
                }
                else
                {
                    if (!antilog)
                        WriteConsole(ConsoleTypes.Error,
                            "An Error occurred trying to enable Logitech Conflict Mode. Error: LGS SDK Library not found (A).");
                }
            }
            else
            {
                //Disable LCC
                if (File.Exists(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll.disabled"))
                {
                    try
                    {
                        //File.Copy(LgsInstall + @"\SDK\LED\x64\LogitechLed.dll.disabled", LgsInstall + @"\SDK\LED\x64\LogitechLed.dll", true);
                        //File.Delete(LgsInstall + @"\SDK\LED\x64\LogitechLed.dll.disabled");
                        FileSystem.RenameFile(_lgsInstall + @"\SDK\LED\x64\LogitechLed.dll.disabled", "LogitechLed.dll");
                        LCCStatus = false;
                    }
                    catch (Exception ex)
                    {
                        if (!antilog)
                            WriteConsole(ConsoleTypes.Error,
                                "An Error occurred trying to enable Logitech Conflict Mode. Error: " + ex.Message);
                        return;
                    }

                    if (!antilog)
                        WriteConsole(ConsoleTypes.Logitech, "Logitech Conflict Mode Disabled.");
                }
                else
                {
                    if (!antilog)
                        WriteConsole(ConsoleTypes.Error,
                            "An Error occurred trying to disable Logitech Conflict Mode. Error: LGS SDK Library not found (B).");
                }
            }
        }

        private void chk_memorycache_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsMemoryCache = chk_memorycache.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_azertymode_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            //ChromaticsSettings.ChromaticsSettingsAzertyMode = chk_azertymode.Checked;
            //SaveChromaticsSettings(1);
        }

        private void cb_qwerty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsQwertyMode = (KeyRegion)cb_qwerty.SelectedIndex;
            Localization.SetKeyRegion(ChromaticsSettings.ChromaticsSettingsQwertyMode);
            SaveChromaticsSettings(1);
        }

        private void chk_reactiveweather_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsReactiveWeather = chk_reactiveweather.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_startupenable_CheckedChanged(object sender, EventArgs e)
        {
            if (chk_startupenable.Checked)
                _rkApp.SetValue("Chromatics", Application.ExecutablePath);
            else
                _rkApp.DeleteValue("Chromatics", false);

            mi_winstart.Checked = chk_startupenable.Checked;
        }

        private void chk_castchargetoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsCastToggle = chk_castchargetoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_castanimatetoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsCastAnimate = chk_castanimatetoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_gcdcounttoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsGcdCountdown = chk_gcdcounttoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_highlighttoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsKeyHighlights = chk_highlighttoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_jobgaugetoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsJobGaugeToggle = chk_jobgaugetoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_keybindtoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsKeybindToggle = chk_keybindtoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_impactflashtog_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsImpactToggle = chk_impactflashtog.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_dfbelltoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsDfBellToggle = chk_dfbelltoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_dev_keyboard_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (chk_dev_keyboard.Checked)
            {
                _deviceKeyboard = true;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceKeyboard = true;
                }

                if (LogitechSdkCalled == 1)
                {
                    _logitechDeviceKeyboard = true;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceKeyboard = true;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    _coolermasterDeviceKeyboard = true;
                }

                if (RoccatSdkCalled == 1)
                {
                    _roccatDeviceKeyboard = true;
                }
                
            }
            else
            {
                _deviceKeyboard = false;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceKeyboard = false;
                }

                if (LogitechSdkCalled == 1)
                {
                    _logitechDeviceKeyboard = false;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceKeyboard = false;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    _coolermasterDeviceKeyboard = false;
                }

                if (RoccatSdkCalled == 1)
                {
                    _roccatDeviceKeyboard = false;
                }
            }

            GlobalResetDevices();
            SaveDevices();
        }

        private void chk_dev_mouse_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;
            
            if (chk_dev_mouse.Checked)
            {
                _deviceMouse = true;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceMouse = true;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceMouse = true;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceMouse = true;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    _coolermasterDeviceMouse = true;
                }

                if (RoccatSdkCalled == 1)
                {
                    _roccatDeviceMouse = true;
                }

            }
            else
            {
                _deviceMouse = false;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceMouse = false;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceMouse = false;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceMouse = false;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    _coolermasterDeviceMouse = false;
                }

                if (RoccatSdkCalled == 1)
                {
                    _roccatDeviceMouse = false;
                }
            }

            GlobalResetDevices();
            SaveDevices();
        }

        private void chk_dev_mousepad_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (chk_dev_mousepad.Checked)
            {
                _deviceMousepad = true;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceMousepad = true;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceMousepad = true;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceMousepad = true;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    //_coolermasterDeviceMousepad = true;
                }

                if (RoccatSdkCalled == 1)
                {
                    //_roccatDeviceMousepad = true;
                }

            }
            else
            {

                _deviceMousepad = false;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceMousepad = false;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceMousepad = false;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceMousepad = false;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    //_coolermasterDeviceMousepad = false;
                }

                if (RoccatSdkCalled == 1)
                {
                    //_roccatDeviceMousepad = false;
                }
            }

            GlobalResetDevices();
            SaveDevices();
        }

        private void chk_dev_headset_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (chk_dev_headset.Checked)
            {
                _deviceHeadset = true;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceHeadset = true;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceHeadset = true;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceHeadset = true;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    //_coolermasterDeviceHeadset = true;
                }

                if (RoccatSdkCalled == 1)
                {
                    //_roccatDeviceHeadset = true;
                }

            }
            else
            {
                _deviceHeadset = false;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceHeadset = false;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceHeadset = false;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceHeadset = false;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    //_coolermasterDeviceHeadset = false;
                }

                if (RoccatSdkCalled == 1)
                {
                    //_roccatDeviceHeadset = false;
                }
            }

            GlobalResetDevices();
            SaveDevices();
        }

        private void chk_dev_keypad_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (chk_dev_keypad.Checked)
            {
                _deviceKeypad = true;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceKeypad = true;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceKeypad = true;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceKeypad = true;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    //_coolermasterDeviceKeypad = true;
                }

                if (RoccatSdkCalled == 1)
                {
                    //_roccatDeviceKeypad = true;
                }

            }
            else
            {
                _deviceKeypad = false;

                if (RazerSdkCalled == 1)
                {
                    _razerDeviceKeypad = false;

                    /*
                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                        */
                }

                if (LogitechSdkCalled == 1)
                {
                    //_logitechDeviceKeypad = false;
                }

                if (CorsairSdkCalled == 1)
                {
                    _corsairDeviceKeypad = false;
                }

                if (CoolermasterSdkCalled == 1)
                {
                    //_coolermasterDeviceKeypad = false;
                }

                if (RoccatSdkCalled == 1)
                {
                    //_roccatDeviceKeypad = false;
                }
            }

            GlobalResetDevices();
            SaveDevices();
        }

        private void chk_keys_singlemode_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (chk_keys_singlemode.Checked)
            {
                _KeysSingleKeyModeEnabled = true;
                cb_singlezonemode.Enabled = true;
            }
            else
            {
                _KeysSingleKeyModeEnabled = false;
                cb_singlezonemode.Enabled = false;
            }

            SetKeysbase = false;
            SaveDevices();
        }

        private void cb_lightbarmode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _lightbarModes)
            {
                if ((string)cb_lightbarmode.SelectedItem != item.Value) continue;
                _LightbarMode = Helpers.ConvertCBToLightbarMode(item.Value);
                break;
            }

            SetKeysbase = false;
            SaveDevices();
        }

        private void cb_singlezonemode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string) cb_singlezonemode.SelectedItem != item.Value) continue;
                _KeysSingleKeyMode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetKeysbase = false;
            SaveDevices();
        }

        private void cb_mouse_z1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_mouse_z1.SelectedItem != item.Value) continue;
                _MouseZone1Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetMousebase = false;
            SaveDevices();
        }

        private void cb_mouse_z2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_mouse_z2.SelectedItem != item.Value) continue;
                _MouseZone2Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetMousebase = false;
            SaveDevices();
        }

        private void cb_mouse_z3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_mouse_z3.SelectedItem != item.Value) continue;
                _MouseZone3Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetMousebase = false;
            SaveDevices();
        }

        private void cb_mouse_zs1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_mouse_zs1.SelectedItem != item.Value) continue;
                _MouseStrip1Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetMousebase = false;
            SaveDevices();
        }

        private void cb_mouse_zs2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_mouse_zs2.SelectedItem != item.Value) continue;
                _MouseStrip2Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetMousebase = false;
            SaveDevices();
        }

        private void cb_pad_zs1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_pad_zs1.SelectedItem != item.Value) continue;
                _PadZone1Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetPadbase = false;
            SaveDevices();
        }

        private void cb_pad_zs2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_pad_zs2.SelectedItem != item.Value) continue;
                _PadZone2Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetPadbase = false;
            SaveDevices();
        }

        private void cb_pad_zs3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_pad_zs3.SelectedItem != item.Value) continue;
                _PadZone3Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetPadbase = false;
            SaveDevices();
        }

        private void cb_headset_z1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_headset_z1.SelectedItem != item.Value) continue;
                _HeadsetZone1Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetHeadsetbase = false;
            SaveDevices();
        }

        private void cb_keypad_z1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_keypad_z1.SelectedItem != item.Value) continue;
                _KeypadZone1Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetKeypadbase = false;
            SaveDevices();
        }

        private void chk_dev_chromalink_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (RazerSdkCalled == 1)
            {
                if (chk_dev_keypad.Checked)
                {
                    _razerDeviceChromaLink = true;
                    _deviceCL = true;
                    SetCLbase = false;

                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));

                }
                else
                {
                    _razerDeviceChromaLink = false;
                    _deviceCL = false;

                    _razer.ResetRazerDevices(_razerDeviceKeyboard, _razerDeviceKeypad, _razerDeviceMouse,
                        _razerDeviceMousepad, _razerDeviceHeadset, _razerDeviceChromaLink,
                        ColorTranslator.FromHtml(ColorMappings.ColorMappingBaseColor));
                }

                SaveDevices();
            }
        }

        private void cb_chromalink_z1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_chromalink_z1.SelectedItem != item.Value) continue;
                _CLZone1Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetCLbase = false;
            SaveDevices();
        }

        private void cb_chromalink_z2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_chromalink_z2.SelectedItem != item.Value) continue;
                _CLZone2Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetCLbase = false;
            SaveDevices();
        }

        private void cb_chromalink_z3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_chromalink_z3.SelectedItem != item.Value) continue;
                _CLZone3Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetCLbase = false;
            SaveDevices();
        }

        private void cb_chromalink_z4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_chromalink_z4.SelectedItem != item.Value) continue;
                _CLZone4Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetCLbase = false;
            SaveDevices();
        }

        private void cb_chromalink_z5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            foreach (var item in _devModesA)
            {
                if ((string)cb_chromalink_z5.SelectedItem != item.Value) continue;
                _CLZone5Mode = Helpers.ConvertCBToDevMode(item.Value);
                break;
            }

            SetCLbase = false;
            SaveDevices();
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            if (Startup == false) return;

            Process.Start("https://github.com/roxaskeyheart/Chromatics/issues");
        }

        private void btn_doc_Click(object sender, EventArgs e)
        {
            if (Startup == false) return;

            Process.Start("https://github.com/roxaskeyheart/Chromatics/wiki");
        }

        private void btn_web_Click(object sender, EventArgs e)
        {
            if (Startup == false) return;

            Process.Start("https://chromaticsffxiv.com/");
        }

        private void chk_desktopnotify_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsDesktopNotifications = chk_desktopnotify.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_debugopt_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsDebugOpt = chk_desktopnotify.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_lcdtoggle_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            if (LogitechSdkCalled == 1)
            {
                if (!chk_lcdtoggle.Checked)
                {
                    if (LcdSdkCalled == 1)
                    {
                        _lcd.ShutdownLcd();
                        _lcd = null;
                        LcdSdkCalled = 0;
                    }
                }
                else
                {
                    _lcd = LogitechLcdInterface.InitializeLcdSdk();

                    if (_lcd != null)
                    {
                        LcdSdk = true;
                        LcdSdkCalled = 1;
                        WriteConsole(ConsoleTypes.Logitech, "LCD SDK Enabled");
                    }
                }
            }

            ChromaticsSettings.ChromaticsSettingsLcdEnabled = chk_lcdtoggle.Checked;
            SaveChromaticsSettings(1);
        }

        private void chk_showstats_CheckedChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsShowStats = chk_showstats.Checked;
            SetKeysbase = false;
            SaveChromaticsSettings(1);
        }

        private void btn_ffxivcachereset_Click(object sender, EventArgs e)
        {
            var cacheReset =
                MessageBox.Show(
                    @"Are you sure you wish to clear Chromatics cache?",
                    @"Clear Cache?", MessageBoxButtons.OKCancel);
            if (cacheReset != DialogResult.OK) return;

            try
            {
                string enviroment = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

                if (File.Exists(enviroment + @"/signatures-x64.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/signatures-x64.json");
                }

                if (File.Exists(enviroment + @"/structures-x64.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/structures-x64.json");
                }

                if (File.Exists(enviroment + @"/actions.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/actions.json");
                }

                if (File.Exists(enviroment + @"/statuses.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/statuses.json");
                }

                if (File.Exists(enviroment + @"/zones.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/zones.json");
                }

                MessageBox.Show(@"Cache Cleared. Please restart Chromatics.", @"Cache Cleared", MessageBoxButtons.OK);
                Thread.Sleep(1);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Unable to clear cache. Are you running as Administrator? Error: " + ex.StackTrace, @"Unable to clear Cache", MessageBoxButtons.OK);
            }
        }

        private void cb_lang_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Startup == false) return;

            ChromaticsSettings.ChromaticsSettingsLanguage = cb_lang.SelectedIndex;
            SetKeysbase = false;
            SaveChromaticsSettings(1);

            /*
            if (IsAdministrator())
            {
                var enviroment = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

                if (File.Exists(enviroment + @"/signatures-x64.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/signatures-x64.json");
                }

                if (File.Exists(enviroment + @"/structures-x64.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/structures-x64.json");
                }

                if (File.Exists(enviroment + @"/actions.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/actions.json");
                }

                if (File.Exists(enviroment + @"/statuses.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/statuses.json");
                }

                if (File.Exists(enviroment + @"/zones.json"))
                {
                    FileSystem.DeleteFile(enviroment + @"/zones.json");
                }
            }

            WriteConsole(ConsoleTypes.Ffxiv, @"Language change detected. Clearing Cache..");
            */
        }

        private delegate void ResetGridDelegate();

        private delegate void ResetMappingsDelegate();
    }
}