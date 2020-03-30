﻿using System;
using System.IO;
using DarkMultiPlayerCommon;
using DarkNetworkUDP;
using MessageStream2;

namespace DarkMultiPlayerServer.Messages
{
    public class CraftLibrary
    {
        public static void SendCraftList(ClientObject client)
        {
            int numberOfCrafts = 0;
            string craftDirectory = Path.Combine(Server.universeDirectory, "Crafts");
            if (!Directory.Exists(craftDirectory))
            {
                Directory.CreateDirectory(craftDirectory);
            }
            string[] players = Directory.GetDirectories(craftDirectory);
            for (int i = 0; i < players.Length; i++)
            {
                players[i] = players[i].Substring(players[i].LastIndexOf(Path.DirectorySeparatorChar) + 1);
            }
            NetworkMessage newMessage = NetworkMessage.Create((int)ServerMessageType.CRAFT_LIBRARY, 512 * 1024);
            newMessage.reliable = true;
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<int>((int)CraftMessageType.LIST);
                mw.Write<string[]>(players);
                foreach (string player in players)
                {
                    string playerPath = Path.Combine(craftDirectory, player);
                    string vabPath = Path.Combine(playerPath, "VAB");
                    string sphPath = Path.Combine(playerPath, "SPH");
                    string subassemblyPath = Path.Combine(playerPath, "SUBASSEMBLY");
                    bool vabExists = Directory.Exists(vabPath);
                    bool sphExists = Directory.Exists(sphPath);
                    bool subassemblyExists = Directory.Exists(subassemblyPath);
                    mw.Write<bool>(vabExists);
                    mw.Write<bool>(sphExists);
                    mw.Write<bool>(subassemblyExists);
                    if (vabExists)
                    {
                        string[] vabCraftNames = Directory.GetFiles(vabPath);
                        for (int i = 0; i < vabCraftNames.Length; i++)
                        {
                            //We only want the craft names
                            vabCraftNames[i] = Path.GetFileNameWithoutExtension(vabCraftNames[i]);
                            numberOfCrafts++;
                        }
                        mw.Write<string[]>(vabCraftNames);
                    }

                    if (sphExists)
                    {
                        string[] sphCraftNames = Directory.GetFiles(sphPath);
                        for (int i = 0; i < sphCraftNames.Length; i++)
                        {
                            //We only want the craft names
                            sphCraftNames[i] = Path.GetFileNameWithoutExtension(sphCraftNames[i]);
                            numberOfCrafts++;
                        }
                        mw.Write<string[]>(sphCraftNames);
                    }

                    if (subassemblyExists)
                    {
                        string[] subassemblyCraftNames = Directory.GetFiles(subassemblyPath);
                        for (int i = 0; i < subassemblyCraftNames.Length; i++)
                        {
                            //We only want the craft names
                            subassemblyCraftNames[i] = Path.GetFileNameWithoutExtension(subassemblyCraftNames[i]);
                            numberOfCrafts++;
                        }
                        mw.Write<string[]>(subassemblyCraftNames);
                    }
                }
                newMessage.data.size = (int)mw.GetMessageLength();
                ClientHandler.SendToClient(client, newMessage, true);
                DarkLog.Debug("Sending " + client.playerName + " " + numberOfCrafts + " craft library entries");
            }
        }

        public static void HandleCraftLibrary(ByteArray messageData, Connection<ClientObject> connection)
        {
            ClientObject client = connection.state;
            using (MessageReader mr = new MessageReader(messageData.data))
            {
                CraftMessageType craftMessageType = (CraftMessageType)mr.Read<int>();
                string fromPlayer = mr.Read<string>();
                if (fromPlayer != client.playerName)
                {
                    Messages.ConnectionEnd.SendConnectionEnd(client, "Kicked for sending an craft library message for another player");
                    return;
                }
                switch (craftMessageType)
                {

                    case CraftMessageType.UPLOAD_FILE:
                        {
                            CraftType uploadType = (CraftType)mr.Read<int>();
                            string uploadName = mr.Read<string>();
                            byte[] uploadData = mr.Read<byte[]>();
                            string playerPath = Path.Combine(Path.Combine(Server.universeDirectory, "Crafts"), fromPlayer);
                            if (!Directory.Exists(playerPath))
                            {
                                Directory.CreateDirectory(playerPath);
                            }
                            string typePath = Path.Combine(playerPath, uploadType.ToString());
                            if (!Directory.Exists(typePath))
                            {
                                Directory.CreateDirectory(typePath);
                            }
                            string craftFile = Path.Combine(typePath, uploadName + ".craft");
                            File.WriteAllBytes(craftFile, uploadData);
                            DarkLog.Debug("Saving " + uploadName + ", type: " + uploadType.ToString() + " from " + fromPlayer);
                            NetworkMessage newMessage = NetworkMessage.Create((int)ServerMessageType.CRAFT_LIBRARY, 512 * 1024);
                            newMessage.reliable = true;
                            using (MessageWriter mw = new MessageWriter(newMessage.data.data))
                            {
                                mw.Write<int>((int)CraftMessageType.ADD_FILE);
                                mw.Write<string>(fromPlayer);
                                mw.Write<int>((int)uploadType);
                                mw.Write<string>(uploadName);
                                newMessage.data.size = (int)mw.GetMessageLength();
                                ClientHandler.SendToAll(client, newMessage, false);
                            }
                        }
                        break;
                    case CraftMessageType.REQUEST_FILE:
                        {
                            string craftOwner = mr.Read<string>();
                            CraftType requestedType = (CraftType)mr.Read<int>();
                            bool hasCraft = false;
                            string requestedName = mr.Read<string>();
                            string playerPath = Path.Combine(Path.Combine(Server.universeDirectory, "Crafts"), craftOwner);
                            string typePath = Path.Combine(playerPath, requestedType.ToString());
                            string craftFile = Path.Combine(typePath, requestedName + ".craft");
                            if (Directory.Exists(playerPath))
                            {
                                if (Directory.Exists(typePath))
                                {
                                    if (File.Exists(craftFile))
                                    {
                                        hasCraft = true;
                                    }
                                }
                            }
                            NetworkMessage newMessage = NetworkMessage.Create((int)ServerMessageType.CRAFT_LIBRARY, 512 * 1024);
                            newMessage.reliable = true;
                            using (MessageWriter mw = new MessageWriter(newMessage.data.data))
                            {
                                mw.Write<int>((int)CraftMessageType.RESPOND_FILE);
                                mw.Write<string>(craftOwner);
                                mw.Write<int>((int)requestedType);
                                mw.Write<string>(requestedName);
                                mw.Write<bool>(hasCraft);
                                if (hasCraft)
                                {
                                    mw.Write<byte[]>(File.ReadAllBytes(craftFile));
                                    DarkLog.Debug("Sending " + fromPlayer + " " + requestedName + " from " + craftOwner);
                                }
                                newMessage.data.size = (int)mw.GetMessageLength();
                            }
                            ClientHandler.SendToClient(client, newMessage, false);
                        }
                        break;
                    case CraftMessageType.DELETE_FILE:
                        {
                            CraftType craftType = (CraftType)mr.Read<int>();
                            string craftName = mr.Read<string>();
                            string playerPath = Path.Combine(Path.Combine(Server.universeDirectory, "Crafts"), fromPlayer);
                            string typePath = Path.Combine(playerPath, craftType.ToString());
                            string craftFile = Path.Combine(typePath, craftName + ".craft");
                            if (Directory.Exists(playerPath))
                            {
                                if (Directory.Exists(typePath))
                                {
                                    if (File.Exists(craftFile))
                                    {
                                        File.Delete(craftFile);
                                        DarkLog.Debug("Removing " + craftName + ", type: " + craftType.ToString() + " from " + fromPlayer);
                                    }
                                }
                            }
                            if (Directory.Exists(playerPath))
                            {
                                if (Directory.GetFiles(typePath).Length == 0)
                                {
                                    Directory.Delete(typePath);
                                }
                            }
                            if (Directory.GetDirectories(playerPath).Length == 0)
                            {
                                Directory.Delete(playerPath);
                            }
                            //Relay the delete message to other clients
                            NetworkMessage newMessage = NetworkMessage.Create((int)ServerMessageType.CRAFT_LIBRARY, messageData.Length);
                            newMessage.reliable = true;
                            Array.Copy(messageData.data, 0, newMessage.data.data, 0, messageData.Length);
                            ClientHandler.SendToAll(client, newMessage, false);
                        }
                        break;
                }
            }
        }
    }
}

