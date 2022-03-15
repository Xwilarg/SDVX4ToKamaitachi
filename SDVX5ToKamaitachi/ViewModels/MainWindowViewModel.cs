using ReactiveUI;
using SDVX5ToKamaitachi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Windows.Input;

namespace SDVX5ToKamaitachi.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
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
                        scores.Add(new()
                        {
                            matchType = "songTitle",
                            identifier = d[0],
                            difficulty = d[1][..3],
                            lamp = d[3],
                            score = d[5]
                        });
                    }
                    output.scores = scores.ToArray();

                    OutputData = Regex.Unescape(JsonSerializer.Serialize(output, new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    }));
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
