using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Helpers;
using System.Text.RegularExpressions;

namespace AxleLoadSystem.Api.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class TestController : ControllerBase
{
    [HttpGet("[action]")]
    public IActionResult Check()
    {
        string line = "TN12345678,1";//,06/12/2024 7:00,PZ1442,PS2868,PN4086,VI363,8,61,94,10,64,2,24,32,98,68,392,0,0,0,14,RN14,BN19,06/12/2024 7:00,06/12/2024 7:00,06/12/2024 7:00,06/12/2024 7:00,06/12/2024 7:00,06/12/2024 7:00,06/12/2024 7:00";
        CsvHelper csvHelper = new CsvHelper();
        string strRegex = csvHelper.CheckRegexForAxleLoad();
        bool isMatched = false;
        try
        {
            isMatched = new Regex(strRegex).IsMatch(line);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
        return Ok(isMatched);
    }

}
