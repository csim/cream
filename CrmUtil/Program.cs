using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmUtil.Commands;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmUtil
{
    class Program
    {
        public Program()
        {
        }

        public CommandBase GetCommand(string verb, object options)
        {
            if (options == null) return null;
            if (options is UpdateWebResourceOptions)
            {
                return new UpdateWebResource((UpdateWebResourceOptions)options);
            }

            return null;
        }

        static void Main(string[] args)
        {
            var prog = new Program();
            prog.Execute(args);
        }

        private void Execute(string[] args)
        {
            var options = new ProgramOptions();

            if (!CommandLine.Parser.Default.ParseArguments(args, options,
              (verb, subOptions) =>
              {
                  try
                  {
                      var command = GetCommand(verb, subOptions);
                      if (command != null)
                      {
                          command.Execute();
                      }
                  }
                  catch (Exception ex)
                  {
                      Console.WriteLine(ex.ToString());
                  }
              }))
            {
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }
        }
    }
}
