using ReactiveUI;
using SDVX5ToKamaitachi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Windows.Input;

namespace SDVX5ToKamaitachi.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        private readonly Dictionary<string, string> _invalidSongs = new()
        {
            { "ARROW RAIN feat. ayame", "ARROW RAIN" },
            { "トーホータノシ (feat. 抹)", "トーホータノシ feat. 抹" }
        };
        // Others songs that doesn't exists:
        // けもののおうじゃ★めうめう

        public MainWindowViewModel()
        {
            SelectFile = ReactiveCommand.Create(() =>
            {
                GetFile.Handle(Unit.Default).Subscribe(Observer.Create<string>(file =>
                {
                    if (file == null)
                    {
                        return;
                    }
                    var data = File.ReadAllLines(file).Select(x => x.Split(',')).ToList();
                    if (data[0].Length != 10)
                    {
                        throw new InvalidOperationException("CSV is not from SDVX IV");
                    }
                    data.RemoveAt(0);
                    var output = new KamaitachiOutput()
                    {
                        meta = new()
                        {
                            game = "sdvx",
                            playtype = "Single",
                            version = "vivid",
                            service = "e-amusement"
                        }
                    };
                    List<Score> scores = new();
                    foreach (var d in data)
                    {
                        var name = _invalidSongs.ContainsKey(d[0])
                            ? _invalidSongs[d[0]]
                            : d[0];
                        scores.Add(new()
                        {
                            matchType = "songTitle",
                            identifier = name.Replace('，', ','),
                            difficulty = d[1] switch
                            {
                                "GRAVITY" => "GRV",
                                _ => d[1][..3]
                            },
                            lamp = d[3] switch
                            {
                                "PLAYED" => "FAILED",
                                "COMPLETE" => "CLEAR",
                                _ => d[3]
                            },
                            score = int.Parse(d[5])
                        });
                    }
                    output.scores = scores.ToArray();

                    OutputData = JsonSerializer.Serialize(output, new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                    });
                }));
            });
        }

        public Interaction<Unit, string> GetFile { get; } = new();
        public ICommand SelectFile { private set; get; }

        private string _outputData;
        public string OutputData
        {
            get => _outputData;
            set => this.RaiseAndSetIfChanged(ref _outputData, value);
        }
    }
}
