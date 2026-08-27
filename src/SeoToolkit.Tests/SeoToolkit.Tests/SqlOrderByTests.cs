using System.Linq.Expressions;
using Moq;
using NPoco;
using SeoToolkit.Umbraco.SiteAudit.Core.Models.Database;
using SeoToolkit.Umbraco.SiteAudit.Core.Repositories;
using Umbraco.Cms.Infrastructure.Persistence;

namespace SeoToolkit.Tests
{
    [TestFixture]
    public class SqlOrderByTests
    {
        private static Sql<ISqlContext> Sql() =>
            new(Mock.Of<ISqlContext>(), "SELECT * FROM [x] WHERE [RunId] = @0", 1);

        [Test]
        public void PassingAnExpressionVariableToOrderBy_PutsTheLambdaInTheSql()
        {
            // The trap SqlOrderBy exists to prevent, pinned so it cannot come back unnoticed.
            // The typed helper needs its type argument spelled out; an expression in a variable
            // binds to NPoco's OrderBy(params object[]) instead, which stringifies it.
            Expression<Func<SiteAuditResourceEntity, object?>> field = it => it.Url;

            var sql = Sql();
            sql.OrderBy(field);

            Assert.Multiple(() =>
            {
                Assert.That(sql.SQL, Does.Contain("=>"),
                    "if this ever stops being true, NPoco changed and the workaround can be revisited");
                Assert.That(sql.SQL, Does.Not.Contain("[Url]"), "no real column name reaches the SQL");
            });
        }

        [Test]
        public void TheHelperProducesRealColumnNamesAndNoLambdaText()
        {
            var sql = Sql();
            sql.OrderBy(SqlOrderBy.Clause("Url", "Id"));

            Assert.Multiple(() =>
            {
                Assert.That(sql.SQL, Does.Contain("ORDER BY [Url], [Id]"));
                Assert.That(sql.SQL, Does.Not.Contain("=>"));
            });
        }

        [Test]
        public void Clause_QuotesEachColumn()
        {
            Assert.That(SqlOrderBy.Clause("Url", "Id"), Is.EqualTo("[Url], [Id]"));
        }

        [Test]
        public void Clause_KeepsTheDirectionOutsideTheQuoting()
        {
            //[Severity DESC] would be read as a column named "Severity DESC".
            Assert.That(SqlOrderBy.Clause("Severity DESC"), Is.EqualTo("[Severity] DESC"));
        }

        [Test]
        public void Clause_HandlesAMixOfDirections()
        {
            Assert.That(SqlOrderBy.Clause("Severity DESC", "Id"), Is.EqualTo("[Severity] DESC, [Id]"));
        }

        [Test]
        public void Clause_RejectsAnEmptyColumnList()
        {
            Assert.Throws<ArgumentException>(() => SqlOrderBy.Clause());
        }

        [Test]
        public void Clause_RejectsABlankColumn()
        {
            Assert.Throws<ArgumentException>(() => SqlOrderBy.Clause("Id", "  "));
        }
    }
}
