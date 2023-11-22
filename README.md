# Exercise: Create a log from state changes in a functional way

In this project I will try to approach a little functionality I faced
recently, in a **functional** way.

This is a simplification and an abstraction of the actual functionality, not the full picture.
Do not critizie its architecture based on how would you have implemented it.

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


## My take
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