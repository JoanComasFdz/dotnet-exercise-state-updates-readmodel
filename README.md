# Exercise: Create a log from state changes in a functional way

In this project I will try to approach a little functionality I faced
recently, in a **functional** way.

This is a simplification and an abstraction of the actual functionality, not the full picture.
Do not critizie its architecture based on how would you have implemented it.

I will create also a traditional OOP implementation to be able to properly compare both approaches.

I consider traditional OOP to include SOLID principles.

## The functionality
The `hardware-connetion-monitor` service monitors the state of the connection to a hardware unit. When that
connection state changes, it publishes it with the hardware unit id, the new state and the current datetime.
The state can be `CONNECTED`, `DISCONNECTED` or `WAITING`.

The `system-reporter` service is listening to the `hardware-connetion-monitor` state changes and creates a log
of disconnections, which includes the hardware unit id, the state, the start time and the end time.

The log is created as follows:

1. If the last disconnection with the hardware unit id does not exist and the given state is not `CONNECTED`, 
 create a new disconnection with the given hardware unit id, state and the datetime as start time, leaveing end time null.

2. If the last disconnection with the hardware unit id exists and the end time is null, set its end time to the given datetime.

3. If the last disconnection with the hardware unit id exists and the given state is not `CONNECTED`,
 create a new disconnection with the given hardware unit id, state and the datetime as start time, leaveing end time null.

 > Important: There is no case where 2 events come with the same hardware unit id and the same state.

## Implementation constrains
- Only the `system-reporter` will be implemented.
- The events will be simulated via a Web API Controller.
- Since it is a read model, the data should be stored already in the way and format that will be returned when queried.
- Since it is a read model, the Controller method to return the log will not do any complex SQL query nor any in memory
 operation, besides obtaining the data from the DB and returning it. No mapping, no Linq, no loops, no ifs.

## My take on how to implement it
My goal is to create an impure-pure-impure sandwitch as follows:
1. Query the DB
2. Decide what to do
3. Update the DB accordingly

So all the logic to decide how the DB is updated should be in step 2, and it has to return everything that step 3 needs
to do all the DB updates.

To implement that I will try to do a descriminated union so that step 3 can do pattern matching. So let's look at the
cases:

Cases 1 and 3 tell me that it is the same action: Create a new disconnection and return it as action `AddNew`.

For case 2, the endTime has to be updated and returned as `UpdateLast`.
But case 2 and 3 are two consecutive things to do for 1 single event, for example:

> Event comes with state `WAITING` and a disconnection exists with end time `null`.

In this case, the actions are:
1. Update the last disconnection end time to the event datetime.
1. Add new disconnection with state `WAITING`, start time is the event datetime and end time is null.

Finally, there is a case in which the log only has to update the end time of the last disconnection: When the last end time is null and the event state is `CONNECTED`.

So, step 2 return 2 cases:
1. `AddNew`, which carries the new disconnection instance.
1. `UpdateLast` which carries the edited disconnection.
1. `UpdateLastEndTimeAndAddNew`, which carries the update disconnection and the new disconnection instance.


## Technical details
- I have disabled AoT because it just adds noise for this exercise.

## My conclusions
- I liked how the DetermineLogChanges ended up looking. It is quite self explanatory and clear. 
Variable names may be unconventional but help understanding the why.
- On the negative side, I wrote a comment before the last return because i realized it can be tricky
 to understand what is the combination of last disconnection, its end time and the event state.
 - Also, there is no way to know outside that method if all cases are being handled in the switch. I have tried
 with an enum, but then the code gets cumbersome since the type information is duplicated and the swtich needs to cast
 the `logChange` on every case.
 - Maybe its because I am rusty, but I needed to check the switch a number of times because I had some bugs there. I am not
 giving it a lot of importance,  but I need to keep this in mind.
 - USing this functional approach also forces you to create very small classes which are actually integral part of the method.
 this could be considered an unnecessary overhead by traditional OOP practicioners: *Why would I create extra types when I can simply
 call the DbContext here?* This is valid criticism, so more effort has to be put into explaining the benefits. I will need to add
 unit tests for the business logic to increase its justifiability.
- The traditional OOP implementation kinda forced me to create two seaprate variants for the MapPost:
  - Alternative 1: This is not 100% equivalent to the functional approach, since there is no other code in the ontroller than delegating all to the `LogService`.
One of the reasons is because then the `LogService` can be tested in isolation, matching how the functinal approach is tested.
On the otherside, this is a 1:1 interface, meaning the LogUpdater itself could be injected, but most  SOLID enjoyers wouldn't like that,
even though there would not make much of a difference. The fact that the controller MapPost method becomes just a pass-trough method can be considered an unnecessary
indirection or layer, only necessary to avoid integration test for ASP .NET Core.
  - Alternative 2: This is a more direct way to implement it, leaving all the code in the controller. It matches a bit better the functional
approach because there is actual code handling the DBContext. On the negative side, the tests must be done using the integration tests approach
for ASP .NET Core which is not as trivial as unit testing a class (even though its not that complex either).
- When it comes to testing the `LogService`, the following things are necessary:
  - A repository implementation to avoid having to use an InMemory DbContext (not a problem here since the DBContext is alreay in memory,
  but important to note it anywhat because production code won't have InMemory DBContexts). This repository is an unnecessary abstraction layer
  from a functionality point of view, but a requirement from a testing point of view. It is also a subpar solution, since for the case of
  update and add it is less performant because `db.SaveChanges()` is called twice. A potential fix would be to add a method in the repository
  to handle update and add, but then it begs the question: Should the repository really provide this functionality? If so, is the
  `LogService` leaking its implementation details?. Additinally, I won't write unit tests for the repository because they will have no return of investment.
  I know this may be a hot take, but what am I going to test? That Add adds the diconnection to the DbContext? This is so trivial.
  - A mocking framework for the unit tests. This is not really a big thing since everybody assumes this is always needed, but its important to remark
that it isn't necessary with the functional approach.
  - Creating the expected and input data is not enough, the mocking framework has to be used to configure the repository with the returning values.
  - In the OOP the coder MUST check that all the calls to the dependency are performed, and possibliy in a very particular order. It can be argued
  that the test is documenting the behaviour of the SUT. And yet, it feels like forced duplication, almost policing: did the dev of this class
  create a new isntance withouth checking if it already exist?



# Next: Use OneOf
As of November 2023, [OneOf](https://github.com/mcintyre321/OneOf) is the default library to use when implementing discriminated unions.

As a follow-up on the exercise, I have renamed the current system-reporter to system-reporter-v1 and created a v2 project, where
I replaced the custom implementation of DU by the tools available in OneOf.
 
 ## Conclusions, v2
- The DU types can be simplified since I no longer need to make them inherit from the same base.
- It could be argue that the Last and the Instance properties could be moved to a single one in LogChange base class, but that would
 decrease understandability because it would need a generic name that wont match either option.
- OneOf provides a Switch method, which is the one that will give you the ability to know if you missed a type on coding and compile time.
- This made me realized that I am passing the entity from DBContext into the business logic, so there is no need to get it again from DB,
i can just return the `last` parameter as part of the result type when needed. Its `EndTime` can therefore be updated outseide in the switch
and the changes will be made effective on `db.SaveChanges()`.
- Overall is less code and more compact.


# Next: High order functions
The next follow-up is to invert the game: Replace discriminated unions by parameters of type `Actions<T>`, which will provide
the necessary functionality to the pure function.

This, in my opinion, implies changing the name of the function, since it no longer just determines something, but also calls
the necessary labdas to make it happen.

## Conclusions, v3
- It is na interesting exercise, but generates too many second-guesses.
- Readability of the `LogChanges` is more ore less the same, but when using only Funcs and Actions, a lot of information about how to use
the params is lost. Because of that I created the delegates with proper documentation. So the DU types are replaced by delegates.
- The testability of the `LogChanges` is decreased in my opinion, since "isCalled" and storage vars are needed just for the sake of checking
what has been done and what has not been done. This is similar to the traditional OOP version.
- The readability of the Controller, stays more or less the same once is written: its more compact but at the same time there are 4 lambdas
using the DbContext and their parameters are not really descriptive since they are named individually. I considered following the apporach of
"blablaBecauseOf" but that would create very long lambdas and all devs are used to short param names in lambdas.
- The writability in the Controller is decreased, because the dev has to understand what the hell are those lambdas for. IntelliSense isn't 
showing the XML documentation of the delegates so it does not help. A mitigation could be to declare the methods to be used in the lambdas...
but then the Controller explodes, which is exactly what I am trying to avoid.
- TL;DR I would **not** choose this approach for this particular use case.

---

# Sumary
I am going to count the files and lines of code to compare both approaches, so the following items are common in both solutions and won't be counted:
 - Disconnection
 - DisconnectionDBContext
 - Program

### OOP
Production code items:
- ILogService => Not necessary, 1:1 interface, 4 lines
- LogService => Not necessary, but facilitates testability, 33 lines.
- IDisconnectionRepository => not necessary for prod, but for testing, 6 lines.
- DisconnctionRepository => overkill with pass-through methods and less performant unless details are leaked, 22 lines.
 Total: 65 lines.

Code in controller:
- MapPost() becomes a pass-through method, 1 line. Good if you fvor hexagonal architecture, doesn't reduce complexity otherwise.
- MapPost(2) forces integration tests for ASP .NET Core but gets rid of 78 lines in exchange for 22 lines in the method and not respecting SOLID.
  
Tests:
- Requires mocking framework to test business logic.
- First test requires 2 lines for instantiation and 1 line to assert the `repo.GetLast...()` is called.
- Second test requires additiinal 2 lines for repo setup.

Total lines needed: 
1. 1 (`MapPost()`) + 65 (items) + 13 (Test1) + 20 (Test2) = 99.
2. 22 (`MapPost()`) + 13 (Test1) + 20 (Test2) = 55.

### Functional (v2)
Production code items:
- BusinessLogic => no way to ensure parameters are inmutable (for now), 23 lines
- AddNew => very small "result" type, 4 lines
- UpdateLastEndTime => very small "result" type, 5 lines
- UpdateLastEndTimeAndAddNew => very small "result" type, 6 lines
Total: 38 lines.

Code in controller:
- MapPost() handles data persistency optimizing calls to `db.SaveChanges()`, doesn't fit hexagonal architecture, 12 lines.

Tests:
- No additional packages to test the business logic.
- First test => 10 lines.
- Second test => 16 lines.

Total lines needed:
11 (`MapPost()`) + 38 (items) + 10 (Test1) + 16 (Test2) = 75.


## TL;DR (when working with (small?) services)

Traditional OOP:
- Requires shallow classes and 1:1 interfaces that don't bring much value, they are there only to adhere to a particular architecture.
- It can be underperformant unless one class leaks details to another, thus violating SOLID.
- There is a way to reduce that complexity, at the cost of having to write integration tests. This is not a big deal nowadays, but it feels wrong to bootstrap the whole service to test some functionality.

Functional:
- Requires an extra library for Discriminated Unions to work but it gets rid of shallow classes and 1:1 interfaces.
- Requires 25% less code (99 to 75 lines) and none of the items are required by external factors (like testability).
- Testing business logic does not require an extra library nor any setuo beyond what the method needs.

We need to talk about SOLID.

