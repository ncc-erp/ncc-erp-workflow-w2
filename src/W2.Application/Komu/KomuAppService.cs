using Elsa;
using Elsa.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using W2.TaskActions;
using W2.TaskEmail;
using W2.Tasks;
using W2.Utils;
using W2.WorkflowDefinitions;
using W2.WorkflowInstances;
namespace W2.Komu
{
    [Authorize]
    public class KomuAppService: W2AppService, IKomuAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly Configurations.KomuConfiguration _komuConfiguration;
        private readonly ILogger<KomuAppService> _logger;
        private readonly IRepository<W2KomuMessageLogs, Guid> _W2KomuMessageLogsRepository;
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IRepository<W2Task, Guid> _taskRepository;
        private readonly IRepository<WorkflowCustomInputDefinition, Guid> _workflowCustomInputDefinitionRepository;
        private readonly IRepository<WorkflowInstanceStarter, Guid> _instanceStarterRepository;
        private readonly IRepository<W2TaskEmail, Guid> _taskEmailRepository;
        private readonly IRepository<W2TaskActions, Guid> _taskActionsRepository;
        private readonly ITaskAppService _taskAppService;

        public KomuAppService(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<KomuAppService> logger,
            IOptions<Configurations.KomuConfiguration> komuConfigurationOptions,
            IRepository<W2KomuMessageLogs, Guid> W2KomuMessageLogsRepository,
            IRepository<W2Task, Guid> taskRepository,
            IRepository<W2TaskActions, Guid> taskActionsRepository,
            IRepository<W2TaskEmail, Guid> taskEmailRepository,
            ITaskAppService taskAppService
            )
        
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _komuConfiguration = komuConfigurationOptions.Value;
            _W2KomuMessageLogsRepository = W2KomuMessageLogsRepository;
            _taskRepository = taskRepository;
            _taskActionsRepository = taskActionsRepository;
            _taskEmailRepository = taskEmailRepository;
            _taskAppService = taskAppService;
    }

        [AllowAnonymous]
        public async Task<List<KomuMessageLogDto>> GetKomuMessageLogListAsync(
            string userName, 
            [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Invalid Date yyyy-MM-dd")]
            string fromTime,
            [RegularExpression("^\\d{4}\\-(0[1-9]|1[012])\\-(0[1-9]|[12][0-9]|3[01])$", ErrorMessage = "Invalid Date yyyy-MM-dd")]
            string toTime)
        {
            IQueryable<W2KomuMessageLogs> queryableLogs = await _W2KomuMessageLogsRepository.GetQueryableAsync();

            if (!string.IsNullOrEmpty(userName))
            {
                queryableLogs = queryableLogs.Where(log => log.SendTo == userName);
            }

            if (!string.IsNullOrEmpty(fromTime) && DateTime.TryParse(fromTime, out DateTime fromDateTime))
            {
                fromDateTime = fromDateTime.Date;
                queryableLogs = queryableLogs.Where(log => log.CreationTime >= fromDateTime);
            }

            if (!string.IsNullOrEmpty(toTime) && DateTime.TryParse(toTime, out DateTime toDateTime))
            {
                toDateTime = toDateTime.Date.AddDays(1).AddTicks(-1);
                queryableLogs = queryableLogs.Where(log => log.CreationTime <= toDateTime);
            }

            List<W2KomuMessageLogs> filteredLogs = await queryableLogs.ToDynamicListAsync<W2KomuMessageLogs>();

            List<KomuMessageLogDto> komuMessageLogDto = ObjectMapper.Map<List<W2KomuMessageLogs>, List<KomuMessageLogDto>>(filteredLogs);

            return komuMessageLogDto;
        }

        [RemoteService(IsEnabled = false)]
        [AllowAnonymous]
        public async Task KomuSendMessageAsync(string username, Guid creatorId, string message )
        {
            if(!String.IsNullOrEmpty(username))
            {
                var komuApiUrl = _komuConfiguration.ApiUrl;
                var komuXSecretKey = _komuConfiguration.XSecretKey;
                
                var requestData = new
                {
                    username,
                    message
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, komuApiUrl + "sendMessageToUser")
                {
                    Content = jsonContent
                };

                request.Headers.Add("X-Secret-Key", komuXSecretKey);

                try
                {
                    var systemResponse = await _httpClient.SendAsync(request);
                    await _W2KomuMessageLogsRepository.InsertAsync(new W2KomuMessageLogs {
                        SendTo = username,
                        Message = message,
                        SystemResponse = systemResponse.ToString(),
                        Status = 1,
                        CreatorId = creatorId,
                        CreationTime = DateTime.Now
                    });

                }
                catch (Exception ex)
                {
                    await _W2KomuMessageLogsRepository.InsertAsync(new W2KomuMessageLogs
                    {
                        SendTo = username,
                        Message = message,
                        SystemResponse = ex.Message,
                        Status = 0,
                        CreatorId = creatorId,
                        CreationTime = DateTime.Now
                    });

                    _logger.LogException(ex);
                }
            }
        }
        [RemoteService(IsEnabled = false)]
        [AllowAnonymous]
        public async Task KomuSendTaskAssignAsync(string username, Guid creatorId, string message)
        {
            if (!String.IsNullOrEmpty(username))
            {
                var komuApiUrl = _komuConfiguration.ApiUrl;
                var komuXSecretKey = _komuConfiguration.XSecretKey;
                var taskDetail = await _taskAppService.GetDetailByIdAsync(message);
             
                
                var requestData = new
                {
                    username,
                    taskDetail
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, komuApiUrl)
                {
                    Content = jsonContent
                };

                request.Headers.Add("X-Secret-Key", komuXSecretKey);

                try
                {
                    var systemResponse = await _httpClient.SendAsync(request);
                    await _W2KomuMessageLogsRepository.InsertAsync(new W2KomuMessageLogs
                    {
                        SendTo = username,
                        Message = JsonConvert.SerializeObject(message),
                        SystemResponse = systemResponse.ToString(),
                        Status = 1,
                        CreatorId = creatorId,
                        CreationTime = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    await _W2KomuMessageLogsRepository.InsertAsync(new W2KomuMessageLogs
                    {
                        SendTo = username,
                        Message = JsonConvert.SerializeObject(message),
                        SystemResponse = ex.Message,
                        Status = 0,
                        CreatorId = creatorId,
                        CreationTime = DateTime.Now
                    });

                    _logger.LogException(ex);
                }
            }
        }
        public async Task<TaskDetailDto> GetDetailByIdAsync(string id)
        {
            var myTask = await _taskRepository.FirstOrDefaultAsync(x => x.Id == Guid.Parse(id));
            var taskAction = await _taskActionsRepository.GetListAsync(x => x.TaskId == id.ToString());
            var taskEmail = await _taskEmailRepository.GetListAsync();
            var query = from task in new List<W2Task> { myTask }
                        join email in taskEmail on new { TaskID = task.Id.ToString() } equals new { TaskID = email.TaskId }
                        join action in taskAction on task.Id.ToString() equals action.TaskId into actionGroup
                        let emailList = (
                             from email in taskEmail
                             where email.TaskId == task.Id.ToString()
                             select email.Email
                         ).ToList()
                        let actionList = (
                             from action in actionGroup.DefaultIfEmpty()
                             select action != null ? new TaskActionsDto
                             {
                                 OtherActionSignal = action.OtherActionSignal,
                                 Status = action.Status
                             } : null
                         ).OrderBy(action => action?.OtherActionSignal).ToList()
                        select new
                        {
                            W2task = task,
                            EmailTo = emailList,
                            OtherActionSignals = actionList.All(a => a != null) ? actionList : null
                        };


            var workflowInstanceId = myTask.WorkflowInstanceId;
            var workflowInstance = await _workflowInstanceStore.FindByIdAsync(workflowInstanceId);

            var data = workflowInstance.Variables.Data;

            var taskDto = ObjectMapper.Map<W2Task, W2TasksDto>(query.FirstOrDefault()?.W2task);
            // todo refactor later 
            // get all defines
            var allDefines = (await _workflowCustomInputDefinitionRepository.GetQueryableAsync())
                .Where(i => i.WorkflowDefinitionId == taskDto.WorkflowDefinitionId)
                .ToDictionary(x => x.WorkflowDefinitionId, x => x);
            var customInput = (await _instanceStarterRepository.GetQueryableAsync())
                .Where(i => i.WorkflowInstanceId == taskDto.WorkflowInstanceId).FirstOrDefault();

            if (customInput != null && allDefines.ContainsKey(taskDto.WorkflowDefinitionId))
            {
                var titleFiled = allDefines.GetItem(taskDto.WorkflowDefinitionId);
                // render title by titleFiled.TitleTemplate
                var InputClone = new Dictionary<string, string>(customInput.Input)
                    {
                        { "RequestUser", taskDto.AuthorName }
                    };
                var title = TitleTemplateParser.ParseTitleTemplateToString(titleFiled.Settings.TitleTemplate, InputClone);
                taskDto.Title = title;
            }

            var taskDetailDto = new TaskDetailDto
            {
                Tasks = taskDto,
                OtherActionSignals = query.FirstOrDefault()?.OtherActionSignals,
                EmailTo = query.FirstOrDefault()?.EmailTo,
                Input = data,
            };

            return taskDetailDto;
        }

    }

}
