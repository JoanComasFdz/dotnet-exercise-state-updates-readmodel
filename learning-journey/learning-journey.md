# Learning journey
Here are all my thougths about how did I slowly evolve the functional apporach.

## v1: First attempt at a functional approach
Here my goal is to isolate the business logic in a static, pure function, returning a discriminated union that I can switch over in the controller.

## My conclusions, v1
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
  - Alternative 1: This is not 100% equivalent to the functional approach, since there is no other code in the ontroller than delegating all to the
 `LogService`. One of the reasons is because then the `LogService` can be tested in isolation, matching how the functinal approach is tested.
On the otherside, this is a 1:1 interface, meaning the LogUpdater itself could be injected, but most  SOLID enjoyers wouldn't like that,
even though there would not make much of a difference. The fact that the controller MapPost method becomes just a pass-trough method can be 
considered an unnecessary indirection or layer, only necessary to avoid integration test for ASP .NET Core.
  - Alternative 2: This is a more direct way to implement it, leaving all the code in the controller. It matches a bit better the functional
approach because there is actual code handling the DBContext. On the negative side, the tests must be done using the integration tests approach
for ASP .NET Core which is not as trivial as unit testing a class (even though its not that complex either).
- When it comes to testing the `LogService`, the following things are necessary:
  - A repository implementation to avoid having to use an InMemory DbContext (not a problem here since the DBContext is alreay in memory,
  but important to note it anywhat because production code won't have InMemory DBContexts). This repository is an unnecessary abstraction layer
  from a functionality point of view, but a requirement from a testing point of view. It is also a subpar solution, since for the case of
  update and add it is less performant because `db.SaveChanges()` is called twice. A potential fix would be to add a method in the repository
  to handle update and add, but then it begs the question: Should the repository really provide this functionality? If so, is the
  `LogService` leaking its implementation details?. Additinally, I won't write unit tests for the repository because they will have no return of
 investment. I know this may be a hot take, but what am I going to test? That Add adds the diconnection to the DbContext? This is so trivial.
  - A mocking framework for the unit tests. This is not really a big thing since everybody assumes this is always needed, but its important to remark
that it isn't necessary with the functional approach.
  - Creating the expected and input data is not enough, the mocking framework has to be used to configure the repository with the returning values.
  - In the OOP the coder MUST check that all the calls to the dependency are performed, and possibliy in a very particular order. It can be argued
  that the test is documenting the behaviour of the SUT. And yet, it feels like forced duplication, almost policing: did the dev of this class
  create a new isntance withouth checking if it already exist?

# v2: Use OneOf
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
- Unit testing can be improved, `IsT0` doesn't mean anything, I could use the switch methof there as well.
- How would stack traces look like if an exceptin happens in any of the lambdas in the `Switch()` method?

# v3: v2 with pattern matching
After further learning, I can condense all the ifs into 3 pattern matching statements, to compare how it reads and feels compared to v2.

## Conclusions, v3
- It gets rid of any variable naming, transforming statements into expressions.
- It needs some getting used to this way of writing code.
- C# syntax to do pattern matching over a tuple may not be the best one.
- In v2 I could have avoided writing variables, but I felt it would be easier to read and mantain; In v3 variable names aren't even possible,
lines feel too long, but at the same time there is a lot going on on a few lines of code.
- This feels truly different than anything I have donde before and closer to whatever functional code I see.
- Unit tests keep the same issue about `IsT0`. But I changed one of the unit tests to get a feeling of how would it lookt like with the switch.

# v4: v3 with no custom types
I want to try to implement it without the custom types, just returning a OneOf of several options.

## Conclusions, v4
- The `DetermineLogChanges` method looks neat, very compact, a lot going on in a few lines of code.
- Lines have been reduced down to 11 from 12 (method) + 15 (return types) = 27, meaning 60% less code.
- The readability of the method itself does not change much thanks to being able to name the properties of the tuples.
- As a consumer, I think the user experience is decreased: To properly use this method, I think it requires XML documentation, explaingin what each 
type in the OneOf represents or is for.
- This begs the question: Is it better to have a longer class and helper types with no XML doc or a shorter class with no helper types with XML doc?
Right now I am not sure.
- Unit tests keep the same issue about `IsT0`.

# v5: High order functions
The next follow-up is to invert the game: Replace discriminated unions by parameters of type `Actions<T>`, which will provide
the necessary functionality to the pure function.

This, in my opinion, implies changing the name of the function, since it no longer just determines something, but also calls
the necessary labdas to make it happen.

## Conclusions, v5
- It is an interesting exercise, but generates too many second-guesses.
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