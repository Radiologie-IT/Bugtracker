using Bugtracker.Attributes;
using Bugtracker.Configuration;
using Bugtracker.Problem_Descriptors;
using System.Collections.Generic;
using System.Text;

namespace Bugtracker.Console.Commands.problem
{
    [Command("problem", "prob", "View and select problem categories")]
    [Arguments(new[] { "subcommand" })]
    class ProblemCommand : Command
    {
        public override string Execute()
        {
            return base.Execute();
        }
    }

    [Command("list", "ls", "Lists all configured problem categories", typeof(ProblemCommand))]
    class ProblemListCommand : Command
    {
        public override string Execute()
        {
            var rc = RunningConfiguration.GetInstance();
            var categories = rc.ProblemCategories.ProblemCategories;

            if (categories.Count == 0)
                return "No problem categories configured.";

            var sb = new StringBuilder();
            sb.AppendLine($"Problem categories ({categories.Count}):");

            foreach (ProblemCategory category in categories)
            {
                string marker = rc.SelectedProblemCategory?.Name == category.Name ? " [selected]" : "";
                sb.AppendLine($"  {category.Name}{marker}");
                if (category.Descriptions.Count > 0)
                    sb.AppendLine($"    Description: {category.Descriptions[0]}");
                sb.AppendLine($"    Ticket:      {category.TicketAbbreviation}");
            }

            return sb.ToString();
        }
    }

    [Command("select", "sel", "Selects a problem category by name", typeof(ProblemCommand))]
    [Arguments(new[] { "category-name" })]
    class ProblemSelectCommand : Command
    {
        public override string Execute()
        {
            string categoryName = string.Join(" ", GivenArguments);
            ProblemCategory category = RunningConfiguration.GetInstance().ProblemCategories.GetProblemCategoryByName(categoryName);

            if (category == null)
                return $"Category \"{categoryName}\" not found. Use: problem list";

            RunningConfiguration.GetInstance().SelectedProblemCategory = category;
            return $"Selected: \"{category.Name}\" (ticket: {category.TicketAbbreviation})";
        }
    }

    [Command("info", "inf", "Shows the currently selected problem category", typeof(ProblemCommand))]
    class ProblemInfoCommand : Command
    {
        public override string Execute()
        {
            ProblemCategory selected = RunningConfiguration.GetInstance().SelectedProblemCategory;

            if (selected == null)
                return "No problem category selected. Use: problem select <name>";

            var sb = new StringBuilder();
            sb.AppendLine($"Name:        {selected.Name}");
            sb.AppendLine($"Ticket:      {selected.TicketAbbreviation}");

            if (selected.Descriptions.Count > 0)
                sb.AppendLine($"Description: {selected.Descriptions[0]}");

            if (selected.Targets.Count > 0)
            {
                var names = new List<string>();
                foreach (var t in selected.Targets)
                    names.Add(t.Name);
                sb.AppendLine($"Targets:     {string.Join(", ", names)}");
            }
            else
            {
                sb.AppendLine("Targets:     (uses default targets)");
            }

            return sb.ToString();
        }
    }

    [Command("clear", "clr", "Clears the currently selected problem category", typeof(ProblemCommand))]
    class ProblemClearCommand : Command
    {
        public override string Execute()
        {
            RunningConfiguration.GetInstance().SelectedProblemCategory = null;
            return "Problem category selection cleared.";
        }
    }
}
