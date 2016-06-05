﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace ACESERVER
{
    class Loops
    {
        public static Stopwatch TickCount = Stopwatch.StartNew();
        public static long Save_Timer = 0;
        public static long Status_Timer = 0;
        public static long Update_Timer = 0;
        public static long Accept_Timer = 0;
        public static string Command = "";
        public static string Last_Packet = "";
        public static void BetaLoop()
        {
            long tmr600;
            long tmr200;
            long Tick;
            tmr200 = 0;
            tmr600 = 0;
            
            do
            {
                Tick = TickCount.ElapsedMilliseconds;
                if (tmr600 < Tick)
                {
                    for (int i = 0; i <= WinsockAsync.Clients.Count - 1; i++)
                    {
                        WinsockAsync.Clients[i].IsConnected = UserConnection.isConnected(i);
                    }
                    tmr600 = Tick + 600;
                }
                if (tmr200 < Tick)
                {

                    if (Status_Timer < TickCount.ElapsedMilliseconds)
                    {
                        Database.StatusAdd("Log do automático do servidor. Gerado na data " + DateTime.Now.ToString());
                        Database.StatusAdd("Existem " + WinsockAsync.Clients.Count + " jogadores online.");
                        Database.StatusAdd("A última atualização de recebimento aconteceu a " + (TickCount.ElapsedMilliseconds - Update_Timer).ToString() + " ms");
                        Database.StatusAdd("A última atualização de conexão aconteceu a " + (TickCount.ElapsedMilliseconds - Accept_Timer).ToString() + " ms");
                        Database.StatusAdd("O último comando chamado no LOOP principal foi: " + Command);
                        Database.StatusAdd("A última packet recebida foi: " + Last_Packet);
                        Status_Timer = TickCount.ElapsedMilliseconds + 300000;
                        Console.WriteLine("Novo log de status gerado.");
                    }
                    tmr200 = Tick + 200;
                }
                Thread.Sleep(10);
            }
            while (true);
        }
        public static void AlphaLoop()
        {

            long tmr100;
            long tmr256;
            long Tick;
            tmr100 = 0;
            tmr256 = 0;
            Database.DefineAdmin();
            WinsockAsync.Log("Servidor iniciado no dia " + DateTime.Now);

            do
            {
                Tick = TickCount.ElapsedMilliseconds;

                //WinsockAsync.Listen();

                Command = "First For";
                for (int i = 1; i <= Globals.Player_HighIndex; i++)
                {
                    if (PStruct.tempplayer[i].ingame) { PStruct.PetMove(i); }
                    if ((PStruct.tempplayer[i].SORE) && (PStruct.character[i, PStruct.player[i].SelectedChar].PVPPenalty < Loops.TickCount.ElapsedMilliseconds))
                    {
                        PStruct.tempplayer[i].SORE = false;
                        PStruct.character[i, PStruct.player[i].SelectedChar].PVPPenalty = 0;
                        SendData.Send_PlayerSoreToMap(i);
                        SendData.Send_Animation(PStruct.character[i, PStruct.player[i].SelectedChar].Map, Globals.Target_Player, i, 148);
                    }
                    for (int p = 1; p < Globals.MaxPassiveEffects; p++)
                    {
                        if ((PStruct.ppassiveffect[i, p].active) && (Tick > PStruct.ppassiveffect[i, p].timer))
                        {
                            PStruct.ExecutePassiveEffect(i, p);
                        }
                    }
                    if (PStruct.tempplayer[i].RegenTimer < Tick)
                    {
                        PStruct.PlayerRegen(i);
                    }
                    if ((PStruct.tempplayer[i].preparingskill > 0) && (Tick > PStruct.tempplayer[i].skilltimer))
                    {
                        PlayerLogic.ExecuteSkill(i);
                    }
                    //Dano sobre tempo
                    for (int s = 1; s < Globals.MaxNTempSpells; s++)
                    {
                        if ((PStruct.ptempspell[i, s].active) && (Tick > PStruct.ptempspell[i, s].timer))
                        {
                            PStruct.ExecuteTempSpell(i, s);
                        }
                    }
                    if ((PStruct.tempplayer[i].Stunned) && (PStruct.tempplayer[i].StunTimer < Tick))
                    {
                        SendData.Send_Stun(PStruct.character[i, PStruct.player[i].SelectedChar].Map, 1, i, 0);
                        PStruct.tempplayer[i].Stunned = false;
                    }
                    if ((PStruct.tempplayer[i].Sleeping) && (PStruct.tempplayer[i].SleepTimer < Tick))
                    {
                        SendData.Send_Sleep(PStruct.character[i, PStruct.player[i].SelectedChar].Map, 1, i, 0);
                        PStruct.tempplayer[i].Sleeping = false;
                    }
                    if ((PStruct.tempplayer[i].Blind) && (PStruct.tempplayer[i].BlindTimer < Tick))
                    {
                        //SendData.Send_Stun(PStruct.character[i, PStruct.player[i].SelectedChar].Map, 1, i, 0);
                        PStruct.tempplayer[i].Blind = false;
                    }
                    if ((PStruct.tempplayer[i].Slowed) && (PStruct.tempplayer[i].SlowTimer < Tick))
                    {
                        if (PStruct.tempplayer[i].isFrozen)
                        {
                            PStruct.tempplayer[i].isFrozen = false;
                            SendData.Send_Frozen(Globals.Target_Player, i);
                        }
                        PStruct.tempplayer[i].Slowed = false;
                        PStruct.tempplayer[i].movespeed = Globals.NormalMoveSpeed;
                        SendData.Send_MoveSpeed(Globals.Target_Player, i);
                    }
                }


                Command = "tmr100";
                if (tmr100 < Tick)
                {
                    for (int i = 0; i <= WinsockAsync.Clients.Count - 1; i++)
                        if (!WinsockAsync.Clients[i].IsConnected)
                        {
                            if (WinsockAsync.Clients[i].Index >= 0)
                            {
                                WinsockAsync.DisconnectUser(i);
                            }
                        }

                    for (int i = 0; i <= Globals.Max_WorkPoints - 1; i++)
                    {
                        if ((MStruct.tempworkpoint[i].respawn > 0) && (Loops.TickCount.ElapsedMilliseconds > MStruct.tempworkpoint[i].respawn))
                        {
                            MStruct.tempworkpoint[i].vitality = MStruct.workpoint[i].vitality;
                            MStruct.tempworkpoint[i].respawn = 0;
                            SendData.Send_EventGraphicToMap(MStruct.workpoint[i].map, MStruct.tile[MStruct.workpoint[i].map, MStruct.workpoint[i].x, MStruct.workpoint[i].y].Event_Id, "", 0, Convert.ToByte(MStruct.workpoint[i].active_sprite));
                        }
                    }

                    tmr100 = Tick + 100;
                }

                Command = "tmr256";

                if (tmr256 < Tick)
                {
                    for (int i = 0; i < Globals.MaxMaps; i++)
                    {
                        MStruct.CheckMapItems(i);

                        if (MStruct.tempmap[i].WarActive)
                        {
                            if (Tick > MStruct.tempmap[i].WarTimer)
                            {
                                bool b = false;
                                for (int i2 = 0; i2 <= MStruct.tempmap[i].NpcCount; i2++)
                                {
                                    if (NStruct.tempnpc[i, i2].guildnum > 0)
                                    {
                                        if (NStruct.tempnpc[i, i2].Target > 0)
                                        {
                                            MStruct.tempmap[i].WarTimer += Tick + 20000;
                                            b = true;
                                        }
                                        NStruct.RegenNpc(i, i2, true);
                                    }
                                }
                                if (b) { break; }
                                SendData.Send_MsgToGuild(MStruct.map[i].guildnum, "O coletor de " + MStruct.map[i].name + " se recuperou do ataque!", Globals.ColorYellow, Globals.Msg_Type_Server);
                                MStruct.tempmap[i].WarActive = false;
                            }
                        }
                        Command = "NPC COUNT";
                        for (int i2 = 0; i2 <= MStruct.tempmap[i].NpcCount; i2++)
                        {
                            if (i2 > 0)
                            {
                                Command = "TIME DAMAGE";
                                //Dano sobre tempo
                                for (int s = 1; s < Globals.MaxNTempSpells; s++)
                                {
                                    if ((NStruct.ntempspell[i, i2, s].active) && (Tick > NStruct.ntempspell[i, i2, s].timer))
                                    {
                                        NStruct.ExecuteTempSpell(i, i2, s);
                                    }
                                }

                                //Processamento do NPC
                                if (MStruct.ExistPlayerInMap(i))
                                {
                                    Command = "NPC MOVE";
                                    NpcIA.CheckNpcMove(i, i2);
                                    if ((NStruct.tempnpc[i, i2].preparingskill > 0) && (Tick > NStruct.tempnpc[i, i2].skilltimer))
                                    {
                                        Command = "NPC EXECUTE SKILL";
                                        NpcIA.ExecuteSkill(i, i2);
                                    }
                                    if (NStruct.tempnpc[i, i2].RegenTimer < Tick)
                                    {
                                        Command = "REGEN NPC";
                                        NStruct.RegenNpc(i, i2);
                                    }
                                    if ((NStruct.tempnpc[i, i2].Stunned) && (NStruct.tempnpc[i, i2].StunTimer < Tick))
                                    {
                                        Command = "NPC STUN";
                                        SendData.Send_Stun(i, 2, i2, 0);
                                        NStruct.tempnpc[i, i2].Stunned = false;
                                    }
                                    if ((NStruct.tempnpc[i, i2].Sleeping) && (NStruct.tempnpc[i, i2].SleepTimer < Tick))
                                    {
                                        Command = "NPC SLEEP";
                                        SendData.Send_Sleep(i, 2, i2, 0);
                                        NStruct.tempnpc[i, i2].Sleeping = false;
                                    }
                                    if ((NStruct.tempnpc[i, i2].Blind) && (NStruct.tempnpc[i, i2].BlindTimer < Tick))
                                    {
                                        Command = "NPC BLIND";
                                        // A fazer SendData.Send_Stun(i, 2, i2, 0);
                                        NStruct.tempnpc[i, i2].Blind = false;
                                    }
                                    if ((NStruct.tempnpc[i, i2].Slowed) && (NStruct.tempnpc[i, i2].SlowTimer < Tick))
                                    {
                                        if (NStruct.tempnpc[i, i2].isFrozen)
                                        {
                                            NStruct.tempnpc[i, i2].isFrozen = false;
                                            SendData.Send_Frozen(Globals.Target_Npc, i2, i);
                                        }
                                        NStruct.tempnpc[i, i2].Slowed = false;
                                        NStruct.tempnpc[i, i2].movespeed = Globals.NormalMoveSpeed;
                                        SendData.Send_MoveSpeed(Globals.Target_Npc, i2, i);
                                    }
                                }
                                else
                                {
                                    Command = "NPC SUPER REGEN";
                                    NStruct.RegenNpc(i, i2, true);
                                    if (NStruct.tempnpc[i, i2].Target > 0) { NStruct.tempnpc[i, i2].Target = 0; }
                                }
                                Command = "NPC RESPAWN";
                                if ((NStruct.tempnpc[i, i2].Dead == true) && (Tick > NStruct.tempnpc[i, i2].RespawnTimer))
                                {
                                    //Respawn
                                    NpcIA.NpcRespawned(i, i2);
                                }
                            }
                        }
                        //for (int i2 = 0; i2 <= MStruct.Ge; i2++)
                        // {
                        //}
                    }
                    tmr256 = Tick + 256;
                }
                long test1 = Tick;
                Command = "Save Timer";
                if (Save_Timer < TickCount.ElapsedMilliseconds)
                {                        //Salva o jogador SE PRECISAR
                    if (WinsockAsync.Clients.Count > 0)
                    {
                        for (int i = 0; i < WinsockAsync.Clients.Count; i++)
                        {
                            if (PStruct.tempplayer[WinsockAsync.Clients[i].Index].ingame)
                            {
                                Database.SaveCharacter(WinsockAsync.Clients[i].Index, PStruct.player[WinsockAsync.Clients[i].Index].Email, PStruct.player[WinsockAsync.Clients[i].Index].SelectedChar);
                                Database.SaveBank(WinsockAsync.Clients[i].Index);
                            }
                        }
                    }
                    Save_Timer = TickCount.ElapsedMilliseconds + 600000;
                    Console.WriteLine("Servidor Salvo.");
                }

                Command = "TPP";
                if ((test1 - Tick) > 0) { Console.WriteLine("TPP: " + (test1 - Tick)); }
                Thread.Sleep(10);
            }
            while (true);
        }
    }
}
