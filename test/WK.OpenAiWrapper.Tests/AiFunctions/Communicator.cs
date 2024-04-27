 using System.Text;

 namespace WK.OpenAiWrapper.Tests.AiFunctions;

 public class Communicator
 {
     public async Task<string> CreateUserStory(string title, string description, 
         string acceptanceCriteria, string releaseNoteDescription)
     {
         try
         {
             return $"https://dev.azure.com/orga/project/_workitems/edit/123";
         }
         catch (Exception ex)
         {
             return ex.Message;
         }
     }

     public async Task<string> CreateBug(string title, string systemInfo, string foundInVersion, 
         string reproSteps, string observedBehaviour, string expectedBehaviour, string diagnosticResults, string errorLocation)
     {
         try
         {
             return $"https://dev.azure.com/orga/project/_workitems/edit/123";
         }
         catch (Exception ex)
         {
             return ex.Message;
         }
     }

     public async Task<string> GetWorkItemInformations(params int[] ids)
     {
         try
         {
             StringBuilder stringBuilder = new();
             stringBuilder.AppendLine($"Start Information ItemId 123");
             stringBuilder.AppendLine($"Title: test title 123");
             stringBuilder.AppendLine($"Description: test Description 123");
             stringBuilder.AppendLine($"State: test State 123");
             stringBuilder.AppendLine($"END Information ItemId 123");

            return stringBuilder.ToString();
         }
         catch (Exception e)
         {
             return e.Message;
         }
     }
 }
