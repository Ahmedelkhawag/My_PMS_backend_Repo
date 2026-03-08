using PMS.Application.DTOs.BackOffice.AR;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PMS.Infrastructure.Documents;

public class CompanySOADocument : IDocument
{
    private readonly CompanyStatementReportDto _data;
    private const string HotelNamePlaceholder = "Hotel";

    public CompanySOADocument(CompanyStatementReportDto data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(24);
                page.PageColor(Colors.White);

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter()
                        .Text(HotelNamePlaceholder)
                        .Style(TextStyle.Default.FontSize(18).Bold());
                    column.Item().PaddingTop(8).AlignCenter()
                        .Text("Statement of Account")
                        .Style(TextStyle.Default.FontSize(14));
                    column.Item().PaddingTop(4).AlignCenter()
                        .Text($"Company: {_data.CompanyName}")
                        .Style(TextStyle.Default.FontSize(12));
                    column.Item().PaddingTop(2).AlignCenter()
                        .Text($"Period: {_data.StartDate:yyyy-MM-dd} to {_data.EndDate:yyyy-MM-dd}")
                        .Style(TextStyle.Default.FontSize(10));
                });

                page.Content().Column(column =>
                {
                    column.Item().PaddingBottom(12).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Opening Balance:").Style(TextStyle.Default.FontSize(10).Bold());
                            c.Item().Text(_data.OpeningBalance.ToString("N2")).Style(TextStyle.Default.FontSize(10));
                        });
                    });

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(90);
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(70);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(6).Text("Date").Style(TextStyle.Default.FontSize(9).Bold());
                            header.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(6).Text("Reference").Style(TextStyle.Default.FontSize(9).Bold());
                            header.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(6).Text("Description").Style(TextStyle.Default.FontSize(9).Bold());
                            header.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignRight().Text("Debit").Style(TextStyle.Default.FontSize(9).Bold());
                            header.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignRight().Text("Credit").Style(TextStyle.Default.FontSize(9).Bold());
                            header.Cell().Background(Colors.Grey.Lighten3)
                                .Padding(6).AlignRight().Text("Balance").Style(TextStyle.Default.FontSize(9).Bold());
                        });

                        foreach (var line in _data.Lines)
                        {
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).Text(line.Date.ToString("yyyy-MM-dd")).Style(TextStyle.Default.FontSize(8));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).Text(line.ReferenceNo).Style(TextStyle.Default.FontSize(8));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).Text(line.Description).Style(TextStyle.Default.FontSize(8));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignRight().Text(line.Debit > 0 ? line.Debit.ToString("N2") : "-")
                                .Style(TextStyle.Default.FontSize(8));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignRight().Text(line.Credit > 0 ? line.Credit.ToString("N2") : "-")
                                .Style(TextStyle.Default.FontSize(8));
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                                .Padding(4).AlignRight().Text(line.Balance.ToString("N2"))
                                .Style(TextStyle.Default.FontSize(8));
                        }
                    });

                    column.Item().PaddingTop(16).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Closing Balance:").Style(TextStyle.Default.FontSize(10).Bold());
                            c.Item().Text(_data.ClosingBalance.ToString("N2")).Style(TextStyle.Default.FontSize(10));
                        });
                    });
                });

                page.Footer().Row(row =>
                {
                    row.RelativeItem().AlignLeft()
                        .Text($"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm}")
                        .Style(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Medium));
                    row.RelativeItem().AlignRight().Text(x =>
                    {
                        x.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Medium));
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
                });
            });
    }
}
