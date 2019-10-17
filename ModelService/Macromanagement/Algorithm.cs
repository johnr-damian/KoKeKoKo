using ModelService.Macromanagement.Types;
using ModelService.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelService.Macromanagement
{
    public partial class Macromanagement
    {
        public List<Tuple<string, CostWorth>> POMDP()
        {
            var battle_result = new List<Tuple<string, CostWorth>>();

            try
            {
                var tree = new POMDP(null);

                int test = 0;
                foreach(var result in tree.GenerateAction())
                {
                    if (test++ == 10)
                        break;

                    battle_result.Add(result);
                }
            }
            catch(ArgumentNullException ex)
            {

            }
            catch(Exception ex)
            {
                battle_result.Clear();
            }

            return battle_result;
        }

        public List<Tuple<string, CostWorth>> MCTS()
        {
            List<Tuple<string, CostWorth>> battle_result = null;

            try
            {

            }
            catch (ArgumentNullException ex)
            {

            }
            catch (Exception ex)
            {
                battle_result.Clear();
            }

            return battle_result;
        }

        public List<Tuple<string, CostWorth>> POMDPMCTS()
        {
            List<Tuple<string, CostWorth>> battle_result = null;

            try
            {

            }
            catch (ArgumentNullException ex)
            {

            }
            catch (Exception ex)
            {
                battle_result.Clear();
            }

            return battle_result;
        }
    }
}
