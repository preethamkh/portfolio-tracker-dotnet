using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PortfolioTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "securities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Exchange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SecurityType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Sector = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Industry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_securities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "price_history",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SecurityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    OpenPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    HighPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    LowPrice = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ClosePrice = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    Volume = table.Column<long>(type: "bigint", nullable: true),
                    PriceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_price_history", x => x.Id);
                    table.ForeignKey(
                        name: "FK_price_history_securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "securities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portfolios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolios_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "holdings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecurityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalShares = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    AverageCost = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holdings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_holdings_portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_holdings_securities_SecurityId",
                        column: x => x.SecurityId,
                        principalTable: "securities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PortfolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalValue = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalGainLoss = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    SnapshotDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolio_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_portfolio_snapshots_portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dividends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HoldingId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountPerShare = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExDividendDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dividends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dividends_holdings_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "holdings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HoldingId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Shares = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    PricePerShare = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    Fees = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transactions_holdings_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "holdings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dividends_holding_id",
                table: "dividends",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "ix_dividends_payment_date",
                table: "dividends",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "ix_holdings_portfolio_id",
                table: "holdings",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "ix_holdings_security_id",
                table: "holdings",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "uq_holdings_portfolio_security",
                table: "holdings",
                columns: new[] { "PortfolioId", "SecurityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_snapshots_portfolio_id",
                table: "portfolio_snapshots",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "uq_snapshots_portfolio_date",
                table: "portfolio_snapshots",
                columns: new[] { "PortfolioId", "SnapshotDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolios_user_default",
                table: "portfolios",
                columns: new[] { "UserId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "ix_portfolios_user_id",
                table: "portfolios",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_price_history_date",
                table: "price_history",
                column: "PriceDate");

            migrationBuilder.CreateIndex(
                name: "ix_price_history_security_id",
                table: "price_history",
                column: "SecurityId");

            migrationBuilder.CreateIndex(
                name: "uq_price_history_security_date",
                table: "price_history",
                columns: new[] { "SecurityId", "PriceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_securities_symbol",
                table: "securities",
                column: "Symbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_securities_type",
                table: "securities",
                column: "SecurityType");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_date",
                table: "transactions",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "ix_transactions_holding_id",
                table: "transactions",
                column: "HoldingId");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dividends");

            migrationBuilder.DropTable(
                name: "portfolio_snapshots");

            migrationBuilder.DropTable(
                name: "price_history");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "holdings");

            migrationBuilder.DropTable(
                name: "portfolios");

            migrationBuilder.DropTable(
                name: "securities");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
