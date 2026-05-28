using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatBot
{
    public delegate void SentimentDetectedHandler(string sentiment, string opener);
    public delegate void MemoryUpdatedHandler(string key, string value);

    public class ChatbotEngine
    {
        public event SentimentDetectedHandler OnSentimentDetected;
        public event MemoryUpdatedHandler     OnMemoryUpdated;

        private readonly Dictionary<string, string> _memory = new Dictionary<string, string>();
        private string _lastTopic = null;
        private readonly Random _rng = new Random();

        // ── Keyword responses ─────────────────────────────────────────────────
        private readonly Dictionary<string, List<string>> _responses =
            new Dictionary<string, List<string>>
        {
            ["phishing"] = new List<string>
            {
                "Phishing is when cybercriminals send fake emails or messages pretending to be a trusted source — like your bank or a popular website — to trick you into revealing passwords, credit card numbers or personal data. Always hover over links before clicking to check the real URL destination.",
                "Phishing attacks often create a false sense of urgency with messages like 'Your account will be closed!' or 'Verify now or lose access!'. Legitimate organisations never pressure you this way. Slow down and verify through official channels before acting.",
                "There are several types of phishing: Email phishing (mass fake emails), Spear phishing (targeted attacks using your personal details), Smishing (fake SMS messages), and Vishing (phone call scams). Always verify the sender's identity independently before sharing anything.",
                "To protect yourself from phishing: never click links in unsolicited emails, go directly to websites by typing the URL yourself, check for HTTPS, use anti-phishing browser extensions, and report suspicious emails to your IT department or email provider."
            },
            ["password"] = new List<string>
            {
                "A strong password is at least 12 characters long and mixes uppercase letters, lowercase letters, numbers and symbols. Never use personal information like your name, birthday or pet's name — these are the first things attackers guess.",
                "Never reuse the same password across multiple accounts. If one site is breached, attackers try that password on every other site — a technique called 'credential stuffing'. Use a unique password for every single account.",
                "A password manager like Bitwarden (free), 1Password or Dashlane generates and securely stores unique passwords for all your accounts. You only need to remember one master password — the manager handles everything else.",
                "Enable two-factor authentication (2FA) on all your accounts in addition to a strong password. Even if your password is stolen, an attacker cannot log in without your second factor — usually an app-generated code."
            },
            ["scam"] = new List<string>
            {
                "Online scams come in many forms: lottery scams ('You've won! Pay a fee to claim'), tech support scams ('Your PC has a virus, call us'), romance scams, and investment fraud. The common thread is a request for money or personal information from someone you have not verified.",
                "If you receive an unexpected call, text or email asking for money, gift cards, or personal details — stop. Hang up or do not reply. Contact the organisation directly using a phone number from their official website to verify if the contact was genuine.",
                "Romance scammers build trust over weeks or months before asking for money, usually with a sudden crisis like a medical emergency or being stranded abroad. Always verify identities through a live video call and never send money to someone you have not met in person.",
                "Report scams to your national cybercrime unit immediately. In South Africa, report to the South African Banking Risk Information Centre (SABRIC) or the SAPS Commercial Crime Unit. Your report helps protect others."
            },
            ["privacy"] = new List<string>
            {
                "Review privacy settings on all your social media accounts right now. Set posts to 'Friends only', disable location sharing in posts, and never publicly display your phone number, home address or workplace — this information can be used for identity theft.",
                "Use a VPN (Virtual Private Network) whenever you connect to public Wi-Fi in places like cafes, airports or hotels. A VPN encrypts your entire internet connection so anyone else on that network cannot intercept your data.",
                "Audit which apps on your phone have access to your camera, microphone, location and contacts. Go to your phone's Settings > Apps > Permissions. Revoke any permissions that the app does not genuinely need to function.",
                "Use a privacy-focused browser like Firefox or Brave combined with the DuckDuckGo search engine. These tools do not track your searches, build advertising profiles, or sell your data to third parties the way mainstream alternatives do."
            },
            ["malware"] = new List<string>
            {
                "Malware is malicious software that includes viruses, spyware, adware and trojans. It can steal your data, encrypt your files, log your keystrokes, or turn your device into part of a criminal network. It usually enters through infected downloads, email attachments or malicious websites.",
                "Keep your operating system and all applications updated at all times. Software updates contain security patches that close the vulnerabilities malware uses to infect your device. Enable automatic updates wherever possible.",
                "Only download software from official sources — the developer's own website, the Microsoft Store, Apple App Store or Google Play. Cracked, pirated, or 'free' versions of paid software almost always contain hidden malware.",
                "Install reputable antivirus software such as Windows Defender (free and built into Windows 10/11), Malwarebytes or ESET. Run full system scans weekly and never dismiss a virus alert without investigating it."
            },
            ["ransomware"] = new List<string>
            {
                "Ransomware is a type of malware that encrypts all the files on your device and then demands payment — usually in cryptocurrency — to restore access. Even if you pay, there is no guarantee your files will be returned. Prevention is the only reliable defence.",
                "Follow the 3-2-1 backup rule to protect against ransomware: keep 3 copies of your data, on 2 different types of media (e.g. hard drive + cloud), with 1 copy stored completely offline or offsite. Ransomware cannot encrypt a backup it cannot reach.",
                "Ransomware almost always arrives through phishing emails with malicious attachments or links. Never open an email attachment you were not expecting, even if it appears to come from someone you know — their account may have been compromised.",
                "If you are hit by ransomware: disconnect your device from the internet and all networks immediately, do not pay the ransom, report it to law enforcement, and restore your files from a clean backup. Contact a cybersecurity professional for recovery assistance."
            },
            ["vpn"] = new List<string>
            {
                "A VPN (Virtual Private Network) creates an encrypted tunnel between your device and the internet, hiding your traffic from your internet provider, hackers on the same network, and websites that track your IP address. It is essential when using public Wi-Fi.",
                "When choosing a VPN, look for: a strict no-logs policy that has been independently audited, strong encryption (AES-256), a kill switch (cuts your internet if the VPN drops), and a reputable company. Reliable options include ProtonVPN, Mullvad and ExpressVPN.",
                "A VPN protects your internet traffic but does not make you fully anonymous. Websites can still identify you through login accounts and browser fingerprinting. For maximum privacy, combine a VPN with a privacy-focused browser and the Tor network for sensitive activities."
            },
            ["two-factor"] = new List<string>
            {
                "Two-factor authentication (2FA) requires you to provide two separate proofs of identity when logging in: something you know (your password) and something you have (a one-time code from your phone). Even if your password is stolen, an attacker cannot access your account without that second code.",
                "Use an authenticator app like Google Authenticator, Microsoft Authenticator or Authy instead of SMS-based 2FA. SMS codes can be intercepted through SIM-swapping attacks where criminals convince your mobile network to transfer your number to their SIM card.",
                "When you enable 2FA, you are given backup codes. Store these in a secure offline location — such as a printed sheet in a locked drawer or an encrypted file. If you lose your phone, backup codes are the only way to regain access to your accounts."
            },
            ["data breach"] = new List<string>
            {
                "A data breach occurs when unauthorised persons access a company's systems and steal user data — which may include your email address, password, phone number, or payment details. Breaches happen regularly at major companies, and your data from old accounts is often at risk.",
                "Visit haveibeenpwned.com right now and enter your email address. This free, trusted service checks your email against hundreds of known breach databases and tells you exactly which sites have exposed your data and what information was leaked.",
                "If your data has been breached: change the affected password immediately, change it on any other site where you used the same password, enable 2FA on the affected account, monitor your bank statements for suspicious transactions, and consider placing a fraud alert with your bank."
            },
            ["firewall"] = new List<string>
            {
                "A firewall is a security system that monitors all network traffic entering and leaving your device or network. It uses a set of rules to block traffic that looks suspicious or unauthorised, acting as a gatekeeper between your device and the internet.",
                "Make sure your computer's built-in firewall is switched on. On Windows 10/11, go to Settings > Windows Security > Firewall and network protection. On macOS, go to System Preferences > Security & Privacy > Firewall. These free built-in firewalls provide solid baseline protection.",
                "For home networks, your router also has a built-in firewall. Log into your router's admin panel (usually 192.168.0.1 or 192.168.1.1) and make sure the firewall is enabled. Change the default admin password if you have not already done so."
            },
            ["browsing"] = new List<string>
            {
                "Always look for HTTPS (the padlock icon) in your browser's address bar before entering any personal, login or payment information. HTTP sites transmit data in plain text that anyone on the same network can read. Never enter sensitive data on HTTP sites.",
                "Install a reputable ad and tracker blocker such as uBlock Origin (free, open-source). Many malicious ads — called 'malvertising' — can infect your device simply by being displayed on a page, without you even clicking them.",
                "Be very cautious about browser extensions. Only install extensions from the official Chrome Web Store or Firefox Add-ons site, check reviews and permission requests carefully, and remove any extensions you no longer use. Malicious extensions can read everything you type."
            },
            ["social engineering"] = new List<string>
            {
                "Social engineering is the art of manipulating people into giving up confidential information or performing actions that compromise security. Unlike hacking, it exploits human psychology — trust, fear, urgency and helpfulness — rather than technical vulnerabilities.",
                "Common social engineering tactics include: pretexting (creating a false scenario to extract info), baiting (leaving infected USB drives for people to find), quid pro quo (offering something in exchange for information), and tailgating (following authorised people through secure doors).",
                "Defend yourself against social engineering by always verifying a person's identity independently before sharing any information or performing any action they request. If someone calls claiming to be from IT support or your bank, hang up and call back on the official number."
            }
        };

        // ── Sentiment openers ─────────────────────────────────────────────────
        private readonly Dictionary<string, string> _sentimentOpeners =
            new Dictionary<string, string>
        {
            ["worried"]     = "It is completely understandable to feel worried — cybersecurity threats are very real. But knowledge is your best protection, and you are already taking the right step by asking.",
            ["scared"]      = "Your concern shows you take your safety seriously — that is the right attitude. Let me give you some practical steps that will help you feel more in control.",
            ["confused"]    = "Cybersecurity can feel overwhelming at first — no worries at all. Let me explain this in clear, simple terms.",
            ["frustrated"]  = "I completely understand the frustration. Security issues are stressful. Let us work through this together, one step at a time.",
            ["curious"]     = "That curiosity is exactly the right attitude! Staying informed and asking questions is one of the most powerful things you can do for your digital safety.",
            ["anxious"]     = "Take a breath — you are already doing the right thing by seeking information. Here is what you need to know.",
            ["overwhelmed"] = "It can feel like a lot to take in at once. Let us focus on the most important step first and build from there."
        };

        private readonly List<string> _followUps = new List<string>
        {
            "tell me more","more","another tip","explain more","go on",
            "continue","elaborate","another one","give me more",
            "keep going","what else","more tips","anything else","next tip"
        };

        private readonly List<string> _greetings = new List<string>
        {
            "hello","hi","hey","good morning","good afternoon",
            "good evening","howdy","greetings","sup"
        };

        public string UserName
        {
            get => _memory.ContainsKey("name") ? _memory["name"] : "";
            set { _memory["name"] = value; OnMemoryUpdated?.Invoke("name", value); }
        }
        public string FavouriteTopic =>
            _memory.ContainsKey("favTopic") ? _memory["favTopic"] : "";

        // ── Main response ─────────────────────────────────────────────────────
        public string GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "Please type a message so I can help you!";

            string low = input.ToLower().Trim();

            // Greeting — whole-word match so "phishing" isn't caught by "hi"
            if (_greetings.Any(g =>
            {
                int idx = low.IndexOf(g);
                if (idx < 0) return false;
                bool beforeOk = idx == 0 || !char.IsLetter(low[idx - 1]);
                bool afterOk  = idx + g.Length >= low.Length || !char.IsLetter(low[idx + g.Length]);
                return beforeOk && afterOk;
            }))
            {
                string n = UserName != "" ? $", {UserName}" : "";
                return $"Hello{n}! I am CYBER-BOT, your cybersecurity advisor.\n\n" +
                       "Ask me about any of these topics:\n" +
                       "  • Passwords\n  • Phishing\n  • Scams\n  • Privacy\n" +
                       "  • Malware\n  • Ransomware\n  • VPN\n  • Two-factor authentication\n" +
                       "  • Data breaches\n  • Firewalls\n  • Safe browsing\n  • Social engineering\n\n" +
                       "Type 'help' to see this list again.";
            }

            // Wellbeing
            if (low.Contains("how are you") || low.Contains("how r u"))
            {
                string n = UserName != "" ? $", {UserName}" : "";
                return $"I am running perfectly{n} and ready to help keep you safe online! What cybersecurity topic would you like to know about?";
            }

            // Help
            if (low == "help" || low.Contains("what can you") || low.Contains("list topics"))
            {
                return "Here are all the topics I can help you with:\n\n" +
                       "  • Passwords\n  • Phishing\n  • Scams\n  • Privacy\n" +
                       "  • Malware\n  • Ransomware\n  • VPN\n  • Two-factor authentication\n" +
                       "  • Data breaches\n  • Firewalls\n  • Safe browsing\n  • Social engineering\n\n" +
                       "Just type your question naturally — I also pick up on how you are feeling!";
            }

            // Name capture
            foreach (string p in new[] { "my name is ", "i am ", "i'm ", "call me " })
            {
                int idx = low.IndexOf(p);
                if (idx >= 0)
                {
                    string after = low.Substring(idx + p.Length).Trim();
                    string name  = after.Split(new[] { ' ', '.', ',', '!' },
                                               StringSplitOptions.RemoveEmptyEntries)
                                        .FirstOrDefault() ?? "";
                    if (name.Length > 1)
                    {
                        name = char.ToUpper(name[0]) + name.Substring(1);
                        UserName = name;
                        return $"Great to meet you, {name}! I will remember your name. Now, what cybersecurity question can I help you with?";
                    }
                }
            }

            // Interest memory
            foreach (string phrase in new[] {
                "i'm interested in ","i am interested in ",
                "i care about ","i want to learn about ","tell me about " })
            {
                if (low.Contains(phrase))
                {
                    string after = low.Substring(low.IndexOf(phrase) + phrase.Length).Trim().TrimEnd('.');
                    string kw    = _responses.Keys.FirstOrDefault(k => after.Contains(k));
                    if (kw != null)
                    {
                        _memory["favTopic"] = kw;
                        OnMemoryUpdated?.Invoke("favTopic", kw);
                        _lastTopic = kw;
                        string n = UserName != "" ? $", {UserName}" : "";
                        return $"Great{n}! I will remember that you are interested in {kw}.\n\n" + RandomTip(kw);
                    }
                }
            }

            // Follow-up
            if (_followUps.Any(t => low.Contains(t)))
            {
                if (_lastTopic != null && _responses.ContainsKey(_lastTopic))
                {
                    string prefix = FavouriteTopic == _lastTopic
                        ? $"As someone interested in {_lastTopic}, here is another tip:\n\n"
                        : $"Here is another tip on {_lastTopic}:\n\n";
                    return prefix + RandomTip(_lastTopic);
                }
                return "I do not have an active topic to continue. Please ask a cybersecurity question first, or type 'help' to see all topics.";
            }

            // Sentiment detection
            foreach (var kvp in _sentimentOpeners)
            {
                if (low.Contains(kvp.Key))
                {
                    OnSentimentDetected?.Invoke(kvp.Key, kvp.Value);
                    string kw = DetectKeyword(low) ?? _lastTopic;
                    if (kw != null)
                    {
                        _lastTopic = kw;
                        return kvp.Value + "\n\n" + RandomTip(kw);
                    }
                    return kvp.Value + "\n\nFeel free to ask about any cybersecurity topic — I am here to help!";
                }
            }

            // Keyword detection — checked LAST so "what is phishing" hits the keyword
            string keyword = DetectKeyword(low);
            if (keyword != null)
            {
                _lastTopic = keyword;
                string personalise = FavouriteTopic == keyword
                    ? $"As someone interested in {keyword}, here is a tip:\n\n"
                    : "";
                return personalise + RandomTip(keyword);
            }

            // Default
            return "I am not sure I understand that. Could you try rephrasing?\n\n" +
                   "Example questions:\n" +
                   "  • \"What is phishing?\"\n" +
                   "  • \"How do I create a strong password?\"\n" +
                   "  • \"Tell me about malware\"\n\n" +
                   "Type 'help' for the full list of topics.";
        }

        private string DetectKeyword(string low) =>
            _responses.Keys.FirstOrDefault(k => low.Contains(k));

        private string RandomTip(string topic) =>
            _responses[topic][_rng.Next(_responses[topic].Count)];
    }
}
