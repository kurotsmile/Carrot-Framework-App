using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class Carrot_Hub : MonoBehaviour
{
    public string SERVER_WORKER_PUBLIC;
    public string SUPABASE_URL;
    public string SUPABASE_ANON_KEY;

    public void ReadTable(string nameTable, UnityAction<string> actDone, UnityAction<string> actErr)
    {
        this.ReadTable(nameTable, null, actDone, actErr);
    }

    public void ReadTable(string nameTable, Dictionary<string, object> filters, UnityAction<string> actDone, UnityAction<string> actErr)
    {
        StartCoroutine(ReadTableS(nameTable, filters, actDone, actErr));
    }

    private IEnumerator ReadTableS(
        string nameTable,
        Dictionary<string, object> filters,
        UnityAction<string> onDone,
        UnityAction<string> onError
    )
    {
        string url = SERVER_WORKER_PUBLIC + "/read_table";

        Dictionary<string, object> data = new()
        {
            { "page", -1 },
            { "limit", 30 },
            { "order_key", "name" },
            { "order_type", "DESC" }
        };

        if (filters != null)
        {
            foreach (var pair in filters) data[pair.Key] = pair.Value;
        }

        Dictionary<string, object> payload = new()
        {
            { "table", nameTable },
            { "data", data }
        };

        string json = Carrot.Json.Serialize(payload);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = new(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(jsonBytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onDone?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke(string.IsNullOrEmpty(req.downloadHandler.text) ? req.error : req.downloadHandler.text);
    }

    // Compatibility layer: accepts previous StructuredQuery JSON and returns Firestore-like runQuery result.
    public void Get_doc(string query, UnityAction<string> act_done = null, UnityAction<string> act_fail = null)
    {
        IDictionary root = Carrot.Json.Deserialize(query) as IDictionary;
        if (root == null)
        {
            act_fail?.Invoke("Invalid query json");
            return;
        }

        IDictionary structured = root["structuredQuery"] as IDictionary;
        if (structured == null)
        {
            act_fail?.Invoke("Missing structuredQuery");
            return;
        }

        string table = this.Get_collection_id(structured);
        if (string.IsNullOrEmpty(table))
        {
            act_fail?.Invoke("Missing collectionId in query");
            return;
        }

        Dictionary<string, object> filters = new()
        {
            { "page", -1 },
            { "limit", 30 },
            { "order_key", "name" },
            { "order_type", "DESC" }
        };

        if (structured["limit"] != null)
        {
            int limit = this.To_int(structured["limit"], 30);
            filters["limit"] = limit;
        }

        this.Fill_order_filter(structured, filters);
        this.Fill_where_filter(structured, filters);

        this.ReadTable(table, filters, s_data =>
        {
            string result = this.To_firestore_query_result(s_data, table);
            act_done?.Invoke(result);
        }, act_fail);
    }

    public string Convert_IDictionary_to_json(IDictionary obj_IDictionary)
    {
        return Carrot.Json.Serialize(obj_IDictionary);
    }

    public void Update_Field_Document(string collectionId, string documentId, string fieldID, string jsonData, UnityAction<string> act_done = null, UnityAction<string> act_fail = null)
    {
        StartCoroutine(UpdateFieldDocumentS(collectionId, documentId, fieldID, jsonData, act_done, act_fail));
    }

    private IEnumerator UpdateFieldDocumentS(string collectionId, string documentId, string fieldID, string jsonData, UnityAction<string> act_done, UnityAction<string> act_fail)
    {
        string url = SERVER_WORKER_PUBLIC + "/update_field_document";

        IDictionary payload = new Dictionary<string, object>
        {
            { "table", collectionId },
            { "id", documentId },
            { "field", fieldID },
            { "data", this.Try_parse_json(jsonData) }
        };

        byte[] body = System.Text.Encoding.UTF8.GetBytes(Carrot.Json.Serialize(payload));
        UnityWebRequest req = new(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            act_done?.Invoke(req.downloadHandler.text);
        else
            act_fail?.Invoke(string.IsNullOrEmpty(req.downloadHandler.text) ? req.error : req.downloadHandler.text);
    }

    public string GetUrlFile(string nameFile)
    {
        if (string.IsNullOrEmpty(nameFile))
            return "";

        if (nameFile.StartsWith("r2:"))
        {
            string realPath = nameFile.Substring(3);
            string encoded = Uri.EscapeDataString(realPath);

            return $"https://json-worker.tranthienthanh93.workers.dev/get_file?file={encoded}";
        }

        return nameFile;
    }

    private string Get_collection_id(IDictionary structured)
    {
        IList fromList = structured["from"] as IList;
        if (fromList == null || fromList.Count == 0) return "";
        IDictionary fromObj = fromList[0] as IDictionary;
        if (fromObj == null || fromObj["collectionId"] == null) return "";
        return fromObj["collectionId"].ToString();
    }

    private void Fill_order_filter(IDictionary structured, Dictionary<string, object> filters)
    {
        IList orderBy = structured["orderBy"] as IList;
        if (orderBy == null || orderBy.Count == 0) return;

        IDictionary order = orderBy[0] as IDictionary;
        if (order == null) return;

        IDictionary field = order["field"] as IDictionary;
        if (field != null && field["fieldPath"] != null)
            filters["order_key"] = field["fieldPath"].ToString();

        if (order["direction"] != null)
        {
            string dir = order["direction"].ToString();
            filters["order_type"] = dir == "ASCENDING" ? "ASC" : "DESC";
        }
    }

    private void Fill_where_filter(IDictionary structured, Dictionary<string, object> filters)
    {
        IDictionary where = structured["where"] as IDictionary;
        if (where == null) return;

        if (where["fieldFilter"] != null)
        {
            this.Apply_field_filter(where["fieldFilter"] as IDictionary, filters);
            return;
        }

        IDictionary composite = where["compositeFilter"] as IDictionary;
        if (composite == null) return;

        IList listFilters = composite["filters"] as IList;
        if (listFilters == null) return;

        for (int i = 0; i < listFilters.Count; i++)
        {
            IDictionary item = listFilters[i] as IDictionary;
            if (item == null) continue;
            this.Apply_field_filter(item["fieldFilter"] as IDictionary, filters);
        }
    }

    private void Apply_field_filter(IDictionary fieldFilter, Dictionary<string, object> filters)
    {
        if (fieldFilter == null) return;
        if (fieldFilter["op"] == null || fieldFilter["op"].ToString() != "EQUAL") return;

        IDictionary field = fieldFilter["field"] as IDictionary;
        IDictionary value = fieldFilter["value"] as IDictionary;
        if (field == null || value == null) return;
        if (field["fieldPath"] == null) return;

        string key = field["fieldPath"].ToString();
        object val = this.Get_query_value(value);
        if (val == null) return;
        filters[key] = val;
    }

    private object Get_query_value(IDictionary value)
    {
        if (value["stringValue"] != null) return value["stringValue"].ToString();
        if (value["integerValue"] != null) return value["integerValue"].ToString();
        if (value["doubleValue"] != null) return value["doubleValue"].ToString();
        if (value["booleanValue"] != null) return value["booleanValue"].ToString().ToLowerInvariant();
        return null;
    }

    private int To_int(object v, int fallback)
    {
        if (v == null) return fallback;
        if (v is int i) return i;
        if (v is long l) return (int)l;
        return int.TryParse(v.ToString(), out int parsed) ? parsed : fallback;
    }

    private object Try_parse_json(string json)
    {
        if (string.IsNullOrEmpty(json)) return "";
        try
        {
            object parsed = Carrot.Json.Deserialize(json);
            return parsed ?? json;
        }
        catch
        {
            return json;
        }
    }

    private string To_firestore_query_result(string s_data, string table)
    {
        IList rows = this.Normalize_rows(s_data);

        List<object> result = new();
        for (int i = 0; i < rows.Count; i++)
        {
            IDictionary row = rows[i] as IDictionary;
            if (row == null) continue;

            string id = row["id"] != null ? row["id"].ToString() : (i + 1).ToString();
            IDictionary fields = this.To_firestore_fields(row);

            IDictionary document = new Dictionary<string, object>
            {
                { "name", "projects/carrotstore/databases/(default)/documents/" + table + "/" + id },
                { "fields", fields }
            };

            IDictionary item = new Dictionary<string, object>
            {
                { "document", document }
            };
            result.Add(item);
        }

        if (result.Count == 0)
        {
            result.Add(new Dictionary<string, object>
            {
                { "readTime", DateTime.UtcNow.ToString("o") }
            });
        }

        return Carrot.Json.Serialize(result);
    }

    private IList Normalize_rows(string s_data)
    {
        object parsed = Carrot.Json.Deserialize(s_data);
        if (parsed is IList list) return list;

        IDictionary obj = parsed as IDictionary;
        if (obj == null) return new List<object>();

        if (obj["results"] is IList results) return results;
        if (obj["data"] is IList data) return data;

        return new List<object>();
    }

    private IDictionary To_firestore_fields(IDictionary source)
    {
        IDictionary fields = new Dictionary<string, object>();
        foreach (var keyObj in source.Keys)
        {
            string key = keyObj.ToString();
            fields[key] = this.To_firestore_value(source[keyObj]);
        }
        return fields;
    }

    private IDictionary To_firestore_value(object value)
    {
        if (value == null)
            return new Dictionary<string, object> { { "stringValue", "" } };

        if (value is IDictionary map)
        {
            return new Dictionary<string, object>
            {
                { "mapValue", new Dictionary<string, object> { { "fields", this.To_firestore_fields(map) } } }
            };
        }

        if (value is IList list)
        {
            List<object> values = new();
            for (int i = 0; i < list.Count; i++)
                values.Add(this.To_firestore_value(list[i]));

            return new Dictionary<string, object>
            {
                { "arrayValue", new Dictionary<string, object> { { "values", values } } }
            };
        }

        return new Dictionary<string, object>
        {
            { "stringValue", value.ToString() }
        };
    }
}
