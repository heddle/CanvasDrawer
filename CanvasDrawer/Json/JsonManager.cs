﻿using System;
using System.Collections.Generic;
using System.Linq;
using CanvasDrawer.DataModel;
using CanvasDrawer.Pages;
using CanvasDrawer.Util;
using CanvasDrawer.Graphics;
using System.Text.Json;
using System.IO;
using System.Text;
using CanvasDrawer.Graphics.Items;

namespace CanvasDrawer.Json {
    public class JsonManager {

        //the page manager
        public PageManager PageManager { get; set; }

        //use thread safe singleton pattern
        private static JsonManager _instance;
        private static readonly object _padlock = new object();

        public delegate void JsonExtRead(string jsonStr);
        public JsonExtRead JsonExtReader { get; set; }

        // delegate will be assigned to the refresher
        public delegate void PageRefresh();
        public PageRefresh Refresher { get; set; }

        //current json (pretty) text
        public string JsonText { get; set; } = "";

        //current json (single string) text
        public string JsonSSText { get; set; } = "";

        JsonManager() : base() {
        }

        /// <summary>
        /// Public access to the singleton
        /// </summary>
        public static JsonManager Instance {
            get {
                lock (_padlock) {
                    if (_instance == null) {
                        _instance = new JsonManager();
                    }
                    return _instance;
                }
            }
        }

        /// <summary>
        /// Reached by CTRL-shift-J but not in reased (REACT) build.
        /// </summary>
        public void FillJSonPage() {
            JsonText = Serialize();
            PageManager.PageChanger("jsonpage");

            if (Refresher != null) {
                Refresher();
            }
        }

        /// <summary>
        /// Format a json string.
        /// </summary>
        /// <param name="jsonStr">The string to format.</param>
        /// <returns>A nicely formatted string.</returns>
        public string Prettify(string jsonStr) {

            JsonDocument doc = JsonDocument.Parse(jsonStr, new JsonDocumentOptions());

            var stream = new MemoryStream();

            JsonWriterOptions opt = new JsonWriterOptions {
                Indented = true
            };

            Utf8JsonWriter writer = new Utf8JsonWriter(stream, opt);
            doc.WriteTo(writer);
            writer.Flush();

            byte[] bytes = stream.ToArray();
            stream.Dispose();
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        } //Prettify

        /// <summary>
        /// Get the global as opposed to the per item properties.
        /// </summary>
        /// <returns>The global properties, e.g., the map name.</returns>
        private Properties GetGlobalProperties() {
            Properties gprops = new Properties();
            gprops.SetGlobal();
            return gprops;
        }

        //deserialize the global properties
        private void DeserializeGlobal(Properties props) {
            Property prop = props.GetProperty(DefaultKeys.MAPNAME);
            if (prop != null) {
                string mName = prop.Value;
                GraphicsManager.Instance.MapName = String.Copy(mName);
            }

            prop = props.GetProperty(DefaultKeys.SHOWGRID);
            if (prop != null) {
                DisplayManager.Instance.ShowGrid = bool.Parse(prop.Value);
            }
            else {
                DisplayManager.Instance.ShowGrid = true;
            }
        }

        /// <summary>
        /// Desrialize a map from local storage.
        /// </summary>
        public void DeserializeFromLocalStorage() {
            string jsonStr = GraphicsManager.Instance.PageManager.LocalStorageGetString(GraphicsManager.Instance.MapName);
            Deserialize(jsonStr);
        }

        
        //JSON serialization
        public string Serialize() {

            //set global properties

            List<Properties> allProps = new List<Properties>();
            allProps.Add(GetGlobalProperties());

            List<Properties> model = ItemManager.Instance.GetModel();

            if (model != null) {
                allProps.AddRange(model);
            }

            string jsonStr = JsonSerializer.Serialize<List<Properties>>(allProps);

            JsonSSText = "jsonSS = \"" + jsonStr.Replace("\"", "\\\"") + "\";";
            jsonStr = JsonManager.Instance.Prettify(jsonStr);

            //put in local storage
            string mapName = string.Copy(GraphicsManager.Instance.MapName);
            GraphicsManager.Instance.PageManager.LocalStoragePutString(mapName, jsonStr);

            return jsonStr;
        }

      
        

        //Deserialize map from single json string
        public void Deserialize(string jsonStr) {
            try {
                var options = new JsonSerializerOptions() {
                    AllowTrailingCommas = true
                };

                List<Properties> propList =
                    JsonSerializer.Deserialize<List<Properties>>(jsonStr, options);

                if ((propList != null) && (propList.Count > 0)) {

                    //delete everything
                    GraphicsManager.Instance.DeleteAllItems();

                    List<Properties> nodeList = new List<Properties>();
                    List<Properties> lineConnectorList = new List<Properties>();
                    List<Properties> nodeBoxList = new List<Properties>();
                    List<Properties> textList = new List<Properties>();


                    //step one, deseialize the global properties
                    foreach (Properties props in propList) {
                        if (props.IsGlobal()) {
                            DeserializeGlobal(props);
                        }
                        else {
                            Property prop = props.GetProperty(DefaultKeys.TYPE_KEY);
                            EItemType type = Item.FromString(prop.Value);

                            if (type == EItemType.Node) {
                                nodeList.Add(props);
                            }
                            else if (type == EItemType.LineConnector) {
                                lineConnectorList.Add(props);
                            }

                            //elbows are deprecated. This is for backwards compatibility
                            else if (type == EItemType.ElbowConnector) {
                                type = EItemType.LineConnector;
                                prop.Value = type.ToString();

                                //remove deprecated property
                                props.Remove(DefaultKeys.HORIZFIRST);
                                //make it a line
                                lineConnectorList.Add(props);
                            }
                            else if (type == EItemType.NodeBox) {
                                nodeBoxList.Add(props);
                            }
                            else if (type == EItemType.Text) {
                                textList.Add(props);
                            }
                        }  //else (not global)
                    } //for each

                    //create the nodes
                    foreach (Properties props in nodeList) {
                        new NodeItem(props);
                    }

                    //create the node boxes
                    foreach (Properties props in nodeBoxList) {
                        new SubnetItem(props);
                    }

                    //create the line connectors
                    foreach (Properties props in lineConnectorList) {
                        new LineConnectorItem(props);
                    }

                    //text annotations
                    foreach (Properties props in textList) {
                        new TextItem(props);
                    }
                }

                //SelectionManager.Instance.UnselectAll();
                //       GraphicsManager.Instance.FullRefresh();

                SharedTimer.Instance.RedrawPending = true;

                //the map is clean at this point
                DirtyManager.Instance.SetClean();
            }
            catch (Exception e) {
                System.Console.WriteLine(e.StackTrace);
            }
        }

        
        /// <summary>
        /// A test map brought up by CTRL-SHIFT-z
        /// </summary>
        /// <returns></returns>
        public String TestCanvas() {
            String jsonSS = "";
            return jsonSS;
        }
    }
}
