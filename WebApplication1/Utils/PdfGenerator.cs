using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using SkiaSharp;
using WebApplication1.Model;

namespace WebApplication1.Utils
{
    public class PdfGenerator : IDocument
    {
        private readonly string _fullName;
        private readonly List<Transaction> _transactions;

        public PdfGenerator(string fullName, List<Transaction> transactions)
        {
            _fullName = fullName;
            _transactions = transactions;
        }
        public byte[] GeneratePdf()
        {
            return Document.Create(Compose).GeneratePdf();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            var now = DateTime.Now;
            var lastMonthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
            var lastMonthEnd = new DateTime(now.Year, now.Month, 1).AddDays(-1);

            var lastMonthTransactions = _transactions
                .Where(t => t.Date.Date >= lastMonthStart.Date && t.Date.Date <= lastMonthEnd.Date)
                .OrderBy(t => t.Date)
                .ToList();

            var totalIncome = lastMonthTransactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            var totalExpense = lastMonthTransactions.Where(t => t.Amount < 0).Sum(t => t.Amount * -1);
            var net = totalIncome - totalExpense;
            var avgPerDay = totalExpense / DateTime.DaysInMonth(lastMonthStart.Year, lastMonthStart.Month);

            container.Page(page =>
            {
                page.Margin(2, Unit.Centimetre);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Content().Column(column =>
                {
                    column.Item().Text($"📄 Group Financial Report for {_fullName}").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    column.Item().Text($"📅 Period: {lastMonthStart:yyyy.MM.dd} – {lastMonthEnd:yyyy.MM.dd}").Italic().FontSize(12).FontColor(Colors.Grey.Darken1);
                    column.Item().PaddingVertical(10).LineHorizontal(1);

                    column.Item().Text("📊 Key Statistics").Bold().FontSize(16).FontColor(Colors.Indigo.Medium);
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(stats =>
                        {
                            stats.Item().Text($"💰 Total Income: {totalIncome:N0} Ft").FontColor(Colors.Green.Darken2);
                            stats.Item().Text($"💸 Total Expenses: {totalExpense:N0} Ft").FontColor(Colors.Red.Darken2);
                            stats.Item().Text($"💼 Net Total: {net:N0} Ft").FontColor(net >= 0 ? Colors.Green.Darken1 : Colors.Red.Darken1);
                            stats.Item().Text($"📅 Avg. Spending / Day: {avgPerDay:N0} Ft").FontColor(Colors.BlueGrey.Darken2);
                        });

                        //row.ConstantItem(200).Image(GenerateChartImage(totalIncome, totalExpense));
                    });

                    column.Item().PaddingVertical(10).LineHorizontal(1);
                    column.Item().Text("🧾 Transactions").Bold().FontSize(16);

                    if (lastMonthTransactions.Count == 0)
                    {
                        column.Item().Text("No transactions recorded in the selected period.").Italic().FontColor(Colors.Grey.Medium);
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Date").Bold();
                                header.Cell().Text("Type").Bold();
                                header.Cell().Text("Category").Bold();
                                header.Cell().Text("Amount").Bold().AlignRight();
                                header.Cell().Text("Description").Bold();
                            });

                            foreach (var t in lastMonthTransactions)
                            {
                                table.Cell().Text(t.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Text(t.Type);
                                table.Cell().Text(t.CategoryId?.ToString() ?? "-");
                                table.Cell().Text($"{t.Amount:N0} Ft").AlignRight().FontColor(t.Amount < 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                                table.Cell().Text(t.Description ?? "-");
                            }
                        });
                    }

                    column.Item().PaddingVertical(10).LineHorizontal(1);
                    column.Item().Text("💡 \"Do not save what is left after spending, but spend what is left after saving.\" – Warren Buffett")
                        .Italic().FontSize(12).FontColor(Colors.Grey.Darken2).AlignCenter();
                });
            });
        }

        
    /*
    private byte[] GenerateChartImage(decimal income, decimal expense)
    {
        var width = 300;
        var height = 300;

        using var surface = SKSurface.Create(new SKImageInfo(width, height));
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };

        float total = (float)(income + expense);
        float angleIncome = 360f * (float)income / total;
        float angleExpense = 360f * (float)expense / total;

        var rect = new SKRect(50, 50, 250, 250);

        // Income (Green)
        paint.Color = SKColors.Green;
        canvas.DrawArc(rect, 0, angleIncome, true, paint);

        // Expense (Red)
        paint.Color = SKColors.Red;
        canvas.DrawArc(rect, angleIncome, angleExpense, true, paint);

        // Labels
        paint.Color = SKColors.Black;
        paint.TextSize = 16;
        canvas.DrawText($"Income", 100, 270, paint);
        canvas.DrawText($"Expense", 180, 270, paint);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
    */
    }
}
