using Microsoft.AspNetCore.Mvc;
using ThePitApi.Models;

namespace ThePitApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DefaultController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("You got it!");
    }

    [HttpPut]
    public IActionResult Put([FromBody] string input)
    {
        return Ok("You put it right" + input);
    }

    [HttpPost]
    public IActionResult Post([FromBody] ContactModel contact)
    {
        return Ok("Thank you" + contact.Name + contact.Email);
    }
}
