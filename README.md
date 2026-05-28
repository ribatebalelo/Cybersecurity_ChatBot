<div align="center">

```
 ██████╗██╗   ██╗██████╗ ███████╗██████╗     ██████╗  ██████╗ ████████╗
██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗    ██╔══██╗██╔═══██╗╚══██╔══╝
██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝    ██████╔╝██║   ██║   ██║   
██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗    ██╔══██╗██║   ██║   ██║   
╚██████╗   ██║   ██████╔╝███████╗██║  ██║    ██████╔╝╚██████╔╝   ██║   
 ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝    ╚═════╝  ╚═════╝    ╚═╝  
```

# Cybersecurity Awareness Chatbot

**A dark-themed WinForms desktop chatbot that teaches cybersecurity through conversation.**
Built with C# · .NET 8 · Windows Forms

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)

</div>

---

## Preview

> Dark cyber-themed interface with chat bubbles, a memory bar, quick-topic buttons, and a blinking secure session indicator.

```
┌─────────────────────────────────────────────────────────────────┐
│  CYBERSECURITY AWARENESS CHATBOT                    ● SECURE    │
│  Keyword recognition • Sentiment detection • Memory recall      │
├─────────────────────────────────────────────────────────────────┤
│   MEMORY:  Name: Alex  | Interest: phishing                  │
├──────────────┬──────────────────────────────────────────────────┤
│ Quick Topics │ password  phishing  scam  privacy  malware  vpn  │
├──────────────┴──────────────────────────────────────────────────┤
│                                                                  │
│   CYBER-BOT                                                    │
│  ─────────────────────────────────────────────────────          │
│  Welcome! I'm here to keep you safe online.                      │
│  What is your name?                                              │
│                                                                  │
│                                              YOU               │
│                                   ─────────────────────         │
│                                            My name is Alex       │
│                                                                  │
├─────────────────────────────────────────────────────────────────┤
│  Type a message...                              [ SEND ► ]       │
├─────────────────────────────────────────────────────────────────┤
│  Response delivered.                         14:32:08 | 28 May  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Features

| Feature | Description |
|---|---|
| **10 Keyword Topics**| Phishing, passwords, malware, ransomware, VPN, 2FA, privacy, scams, firewalls, data breaches |
| **Random Responses**| 3–4 unique responses per topic — never the same answer twice |
| **Memory & Recall**| Remembers your name and favourite topic across the session |
| **Follow-up Detection**| "Tell me more", "another tip", "go on" — it continues from where it left off |
| **Sentiment Detection**| Detects worried, confused, frustrated, curious and responds with empathy first |
| **Dark Cyber Theme**| Deep navy, electric blue, green accents — custom-painted rounded bubbles |
| **Audio Feedback**| WAV sounds on greeting, each response, and session close |
| **Quick Topic Buttons**| One-click buttons to jump straight into any cybersecurity topic |

---

## Project Structure

```
CybersecurityChatBot/
│
├── Program.cs            ← Entry point — boots the WinForms runtime
├── MainForm.cs           ← All UI: layout, bubbles, scrolling, audio, events
├── ChatbotEngine.cs      ← All logic: keywords, responses, memory, sentiment
├── CybersecurityChatBot.csproj  ← SDK-style project file (.NET 8)
│
└── Assets/
    ├── greeting.wav      ← Plays on launch
    ├── question.wav      ← Plays after each bot response
    └── closing.wav       ← Plays on exit
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8) or later
- Windows OS (WinForms is Windows-only)
- Visual Studio 2022 **or**the .NET CLI

### Installation

**1. Clone the repository**
```bash
git clone https://github.com/your-username/CybersecurityChatBot.git
cd CybersecurityChatBot
```

**2. Place your audio assets**

Copy your WAV files into one of these locations (the app checks both automatically):
```
bin\Debug\net8.0-windows\assets\   ← preferred (.NET 8 output folder)
bin\Debug\assets\                  ← also works (legacy location)
```

**3. Build and run**

```bash
dotnet run
```
Or open `CybersecurityChatBot.csproj` in Visual Studio 2022 and press **F5**.

---

## How to Use

| You type... | What happens |
|---|---|
| Your name | Bot greets you by name and remembers it |
| `phishing` / `what is malware?` | Gets a random tip on that topic |
| `tell me more` / `another tip` | Continues from the last topic |
| `I'm worried about scams` | Empathy response + practical tip |
| `I'm interested in VPNs` | Bot remembers your interest for future responses |
| `help` | Lists all available topics |
| `exit` / `bye` / `quit` | Plays closing sound and ends the session |

---

## Architecture

The project follows a clean **separation of concerns**— the UI and the logic never import each other.

```
┌─────────────────────┐        events         ┌──────────────────────┐
│      MainForm       │ ◄──────────────────── │    ChatbotEngine     │
│   (UI layer)        │                        │   (logic layer)      │
│                     │  GetResponse(input)    │                      │
│  • Bubble painting  │ ──────────────────────►│  • Keyword matching  │
│  • Scroll system    │                        │  • Sentiment detect  │
│  • Audio playback   │  OnSentimentDetected   │  • Memory/recall     │
│  • Memory bar       │ ◄──────────────────── │  • Random responses  │
│  • Status strip     │  OnMemoryUpdated       │  • Follow-up logic   │
└─────────────────────┘ ◄──────────────────── └──────────────────────┘
```

### Custom Delegates

```csharp
public delegate void SentimentDetectedHandler(string sentiment, string opener);
public delegate void MemoryUpdatedHandler(string key, string value);
```

The engine fires these events. The form subscribes — that's the only connection between the two layers.

---

## Topics Covered

<details>
<summary><b>Passwords</b></summary>

- Strong password construction (12+ chars, mixed types)
- Credential stuffing and why reuse is dangerous
- Password managers (Bitwarden, 1Password, Dashlane)
- Combining passwords with 2FA
</details>

<details>
<summary><b>Phishing</b></summary>

- How phishing emails work and what to look for
- Spear phishing, smishing, and vishing explained
- Urgency tactics attackers use
- How to verify links and senders safely
</details>

<details>
<summary><b>Malware & Ransomware</b></summary>

- Types of malware: viruses, spyware, trojans, adware
- The 3-2-1 backup rule against ransomware
- Safe download sources and antivirus recommendations
- What to do if you're hit by ransomware
</details>

<details>
<summary><b>VPN & Privacy</b></summary>

- What a VPN does and when to use one
- What to look for in a trustworthy VPN
- Social media privacy settings audit
- Privacy-focused browsers and search engines
</details>

<details>
<summary><b>More topics...</b></summary>

Two-Factor Authentication · Data Breaches · Firewalls · Scams · Safe Browsing · Social Engineering
</details>

---

## Technical Highlights

- **Custom scroll system**— manual `VScrollBar` with a floating "scroll to bottom" button; no AutoScroll quirks
- **Custom bubble rendering**— rounded corners drawn with `GraphicsPath` and four arc calls; no third-party libraries
- **Auto-height RichTextBox**— uses `GetPreferredSize` so bubbles grow with the text
- **Smart asset resolver**— finds audio files in both .NET 8 and legacy output folder layouts
- **`System.Media.SoundPlayer`**— managed async audio; replaces the old `winmm.dll` P/Invoke from .NET Framework
- **Blinking status indicator**— `System.Windows.Forms.Timer` at 900 ms for the "● SECURE SESSION" label

---

## Requirements Checklist

- [x] WinForms GUI with dark cyber theme
- [x] 10 keyword topics with 3–4 random responses each
- [x] Conversation follow-up detection
- [x] Memory and personalisation (`name`, `favTopic`)
- [x] Sentiment detection with empathetic openers
- [x] Custom delegates and events (`SentimentDetectedHandler`, `MemoryUpdatedHandler`)
- [x] Generic collections throughout (`Dictionary<string, List<string>>`, etc.)
- [x] OOP — engine class fully separated from UI class
- [x] Audio: greeting, response, and closing sounds
- [x] Error handling on all audio calls and empty input

---

## Contributing

Pull requests are welcome! For major changes, please open an issue first to discuss what you'd like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/NewTopic`)
3. Commit your changes (`git commit -m 'Add social engineering topic'`)
4. Push to the branch (`git push origin feature/NewTopic`)
5. Open a pull request

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

<div align="center">

**Built to make cybersecurity knowledge accessible to everyone.**

*Stay safe online — a little knowledge goes a long way.*

</div>
