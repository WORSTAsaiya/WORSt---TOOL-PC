using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WorstPC
{
    internal sealed class Account
    {
        public string Key = "";
        public string Username = "";
        public string RbxName = "";
        public bool? Active = null;

        public string Display()
        {
            if (!string.IsNullOrEmpty(RbxName)) return RbxName;
            return string.IsNullOrEmpty(Username) ? "Account" : Username;
        }
        public string StatusText()
        {
            if (Active == null) return "—";
            return Active.Value ? "Active" : "Dead";
        }
    }

    internal static class Api
    {
        public const string BANK = "https://api.worst.homes";
        public const string VAULT = "https://worst-vault.raizelerator.workers.dev";

        public static string LastError = "";

        private static readonly HttpClient Http = BuildClient();

        private static HttpClient BuildClient()
        {
            HttpClientHandler h = new HttpClientHandler();
            h.UseCookies = false;
            h.AllowAutoRedirect = true;
            HttpClient c = new HttpClient(h);
            c.Timeout = TimeSpan.FromSeconds(20);
            return c;
        }

        // ---------- Discord login (same flow the phone app uses) ----------

        public static string Sha256Hex(string s)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] d = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in d) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public static string RandomHex(int bytes)
        {
            byte[] b = new byte[bytes];
            using (RandomNumberGenerator r = RandomNumberGenerator.Create()) r.GetBytes(b);
            StringBuilder sb = new StringBuilder();
            foreach (byte x in b) sb.Append(x.ToString("x2"));
            return sb.ToString();
        }

        // GET /auth/app?h=<sha256(secret)> -> the Discord authorize URL
        public static string AuthStart(string secretHash)
        {
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, BANK + "/auth/app?h=" + secretHash);
                HttpResponseMessage r = Http.Send(m);
                string body = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (!r.IsSuccessStatusCode) { LastError = "Couldn't start login (" + (int)r.StatusCode + ")"; return null; }
                using (JsonDocument doc = JsonDocument.Parse(body))
                {
                    JsonElement url;
                    if (doc.RootElement.TryGetProperty("url", out url)) return url.GetString();
                }
                LastError = "Login start gave no URL";
                return null;
            }
            catch (Exception e) { LastError = "Couldn't reach worst.homes: " + e.Message; return null; }
        }

        // POST /auth/app-claim {code, secret} -> sid
        public static string AuthClaim(string code, string secret)
        {
            try
            {
                string json = JsonSerializer.Serialize(new Dictionary<string, string> { { "code", code }, { "secret", secret } });
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Post, BANK + "/auth/app-claim");
                m.Content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage r = Http.Send(m);
                string body = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using (JsonDocument doc = JsonDocument.Parse(body))
                {
                    JsonElement ok, sid, err;
                    if (!r.IsSuccessStatusCode || !doc.RootElement.TryGetProperty("ok", out ok) || !ok.GetBoolean())
                    {
                        LastError = doc.RootElement.TryGetProperty("error", out err) ? err.GetString() : "Login failed";
                        return null;
                    }
                    if (doc.RootElement.TryGetProperty("sid", out sid)) return sid.GetString();
                }
                LastError = "Login gave no session";
                return null;
            }
            catch (Exception e) { LastError = "Login failed: " + e.Message; return null; }
        }

        // ---------- Account bank ----------

        // POST /bank/list  (Cookie: wsid=...) -> accounts
        public static List<Account> BankList(string wsid)
        {
            List<Account> list = new List<Account>();
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Post, BANK + "/bank/list");
                m.Headers.TryAddWithoutValidation("Cookie", "wsid=" + wsid);
                m.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                HttpResponseMessage r = Http.Send(m);
                string body = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if ((int)r.StatusCode == 401) { LastError = "Log in again."; return null; }
                if (!r.IsSuccessStatusCode) { LastError = "The bank answered " + (int)r.StatusCode; return null; }
                using (JsonDocument doc = JsonDocument.Parse(body))
                {
                    JsonElement accts;
                    if (doc.RootElement.TryGetProperty("accounts", out accts) && accts.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement a in accts.EnumerateArray())
                        {
                            Account ac = new Account();
                            ac.Key = Str(a, "key");
                            ac.Username = Str(a, "username");
                            ac.RbxName = Str(a, "rbxName");
                            JsonElement act;
                            if (a.TryGetProperty("active", out act))
                            {
                                if (act.ValueKind == JsonValueKind.True) ac.Active = true;
                                else if (act.ValueKind == JsonValueKind.False) ac.Active = false;
                            }
                            list.Add(ac);
                        }
                    }
                }
                LastError = "";
                return list;
            }
            catch (Exception e) { LastError = "Couldn't reach the bank: " + e.Message; return null; }
        }

        // POST vault /fetch {key, token, device} -> the .ROBLOSECURITY cookie
        public static string FetchCookie(string key, string token, string device)
        {
            try
            {
                string json = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "key", (key ?? "").Trim() },
                    { "token", token ?? "" },
                    { "device", device ?? "" }
                });
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Post, VAULT + "/fetch");
                m.Content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage r = Http.Send(m);
                string body = r.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                using (JsonDocument doc = JsonDocument.Parse(body))
                {
                    JsonElement ok, ck, err;
                    if (!doc.RootElement.TryGetProperty("ok", out ok) || !ok.GetBoolean())
                    {
                        LastError = doc.RootElement.TryGetProperty("error", out err) ? err.GetString() : "Couldn't get that account.";
                        return null;
                    }
                    if (doc.RootElement.TryGetProperty("cookie", out ck))
                    {
                        string cookie = ck.GetString();
                        if (string.IsNullOrEmpty(cookie)) { LastError = "The saved account was empty."; return null; }
                        return cookie;
                    }
                }
                LastError = "No cookie returned.";
                return null;
            }
            catch (Exception e) { LastError = "Couldn't reach the bank: " + e.Message; return null; }
        }

        // ---------- Roblox launch ticket ----------

        // Exchange a .ROBLOSECURITY cookie for a one-time authentication ticket.
        public static string GetTicket(string cookie)
        {
            try
            {
                string csrf = CsrfProbe(cookie);
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v1/authentication-ticket/");
                m.Headers.TryAddWithoutValidation("Cookie", ".ROBLOSECURITY=" + cookie);
                m.Headers.TryAddWithoutValidation("Origin", "https://www.roblox.com");
                m.Headers.TryAddWithoutValidation("Referer", "https://www.roblox.com/");
                m.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
                if (!string.IsNullOrEmpty(csrf)) m.Headers.TryAddWithoutValidation("X-CSRF-TOKEN", csrf);
                m.Content = new StringContent("", Encoding.UTF8);
                HttpResponseMessage r = Http.Send(m);
                IEnumerable<string> vals;
                if (r.Headers.TryGetValues("rbx-authentication-ticket", out vals))
                    foreach (string v in vals) if (!string.IsNullOrEmpty(v)) return v;
                LastError = "No ticket (cookie dead or rate-limited, status " + (int)r.StatusCode + ")";
                return null;
            }
            catch (Exception e) { LastError = "Ticket failed: " + e.Message; return null; }
        }

        private static string CsrfProbe(string cookie)
        {
            try
            {
                HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Post, "https://auth.roblox.com/v1/authentication-ticket/");
                m.Headers.TryAddWithoutValidation("Cookie", ".ROBLOSECURITY=" + cookie);
                m.Headers.TryAddWithoutValidation("Referer", "https://www.roblox.com/");
                m.Headers.TryAddWithoutValidation("Origin", "https://www.roblox.com");
                m.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0");
                m.Content = new StringContent("", Encoding.UTF8);
                HttpResponseMessage r = Http.Send(m);
                IEnumerable<string> vals;
                if (r.Headers.TryGetValues("x-csrf-token", out vals))
                    foreach (string v in vals) return v;
            }
            catch { }
            return "";
        }

        private static string Str(JsonElement o, string name)
        {
            JsonElement v;
            if (o.TryGetProperty(name, out v) && v.ValueKind == JsonValueKind.String) return v.GetString();
            return "";
        }
    }
}
