# Test cases — Elevator Control System

Fill **Actual result**, **Pass/Fail** and **Screenshot ref** during your own run.

| Test ID | Task | Description | Steps | Expected result | Actual result | Pass/Fail | Screenshot ref |
| --- | --- | --- | --- | --- | --- | --- | --- |
| T01 | GUI | Main window layout | Start the app | Building cross-section, two car panels, hall Up/Down buttons, Log, Refresh and Clear buttons, grid, status labels |  |  |  |
| T02 | GUI | Control names | Open Form1.Designer in Visual Studio | Names such as `buildingView`, `panelA`, `panelB`, `lblStatusA`, `btnLog`, `btnRefreshLog`, `btnClearLog` |  |  |  |
| T03 | GUI | Hall lanterns | Leave cars on floor 1 | Lanterns show 1 and a lime box on floor 1 of each shaft |  |  |  |
| T04 | Logic | Panel A 1→2 | On panel A press 2 | Car A moves up; status `Elevator A: Floor 1 -> Floor 2 (Up)`; display 1 then 2 |  |  |  |
| T05 | Logic | Panel A 1→3 | Return A to 1, press 3 | Status `Elevator A: Floor 1 -> Floor 3 (Up)`; floor number 1→2→3 |  |  |  |
| T06 | Logic | Panel A 2→1 | Take A to 2, press 1 | Status `Elevator A: Floor 2 -> Floor 1 (Down)` |  |  |  |
| T07 | Logic | Panel A 2→3 | Take A to 2, press 3 | Status `Elevator A: Floor 2 -> Floor 3 (Up)` |  |  |  |
| T08 | Logic | Panel A 3→1 | Take A to 3, press 1 | Status `Elevator A: Floor 3 -> Floor 1 (Down)`; passes 2 |  |  |  |
| T09 | Logic | Panel A 3→2 | Take A to 3, press 2 | Status `Elevator A: Floor 3 -> Floor 2 (Down)` |  |  |  |
| T10 | Logic | Same-floor press | While idle at 1, press 1 | Car does not leave floor 1; doors open then close |  |  |  |
| T11 | Logic | Button spam while moving | Start 1→3, mash 2, 3, 1, 2 | No crash; extra floors queued; current trip not cancelled |  |  |  |
| T12 | Hall | Outside buttons | Press Floor 3 then Floor 1 in the hall | Controller assigns a car; both cars can be used |  |  |  |
| T13 | Animation | Car moves in the shaft | Watch shaft A on a 1→3 trip | Glass car slides to each floor Y; timer based, not a jump; doors stay closed while moving |  |  |  |
| T14 | Animation | Arrows | During up / down / idle | Panel shows ▲, ▼, or - ; floor lights match |  |  |  |
| T15 | DB | Connect on start | Run with LocalDB available | No error box; `Data\ElevatorLog.mdf` appears next to the exe |  |  |  |
| T16 | DB | Store rows | Make one trip, wait for doors to close | Table has Request, Moving, Arrived, DoorsOpened, DoorsClosed; ElevatorId 1 or 2 |  |  |  |
| T17 | DB | Retrieve | Click **Log** | Grid fills, newest first, no UI freeze |  |  |  |
| T18 | DB | Disconnected insert | Breakpoint on `adapter.Update` in `SaveLog` | Insert uses DataTable, not `ExecuteNonQuery` |  |  |  |
| T19 | DB | Relative path | Read status bar under Log | Path is `AppContext.BaseDirectory\Data`, no hard-coded `C:\...` in source |  |  |  |
| T20 | DB | Missing .mdf | Close app, delete `ElevatorLog.mdf` if LocalDB has released it, run again | App creates a new file or shows a friendly message and keeps running |  |  |  |
| T21 | DB | LocalDB stopped | `sqllocaldb stop MSSQLLocalDB`, run app | MessageBox, lifts still move |  |  |  |
| T22 | BW | BackgroundWorker save | Breakpoint `SaveWorker_DoWork` during a trip | Save runs off the UI thread; car keeps moving |  |  |  |
| T23 | BW | BackgroundWorker load | Click Log | Button shows Loading… then grid; UI stays responsive |  |  |  |
| T24 | Two cars | Panel isolation | Press 3 on panel B only | Only car B moves |  |  |  |
| T25 | Two cars | Shortest wait | Send A toward 3. While A is moving, press hall Floor 1 (B idle at 1) | B is chosen (cost 0) or the nearer idle car; A is not interrupted |  |  |  |
| T26 | Two cars | Equal cost | Both idle on 1, hall Floor 3 | Elevator A is chosen |  |  |  |
| T27 | State | Queue after doors | Request 3 then quickly 1 | After 3, doors open, then trip to 1 |  |  |  |
| T28 | Observer | Displays follow events | Move A 1→3 | Panel A display, hall lanterns and BuildingView update together |  |  |  |
| T29 | GUI | GDI+ cars | Start the app (no picture files needed) | Two glass cars are drawn in the shafts with letters A and B |  |  |  |
| T30 | GUI | Cars move | Send A to floor 3 and B to floor 2 | Glass cars slide on the same Timer Y values as the floor rows |  |  |  |
| T31 | GUI | No image files | Run on a PC without an Images folder | Building, cars and doors still draw; no missing-file message |  |  |  |
| T32 | GUI | Copy output folder | Copy `bin\Debug\net8.0-windows` elsewhere and run the exe | Building still draws; database path stays relative |  |  |  |
| T33 | Emergency | Idle | Leave A idle at 1, press Emergency A | A stops at once; red text EMERGENCY - stopped at Floor 1; panel shows E |  |  |  |
| T34 | Emergency | Moving up | Send A 1→3, press Emergency between 1 and 2 | A continues to floor 2 then stops; does not stop between floors |  |  |  |
| T35 | Emergency | Moving down | Send A 3→1, press Emergency between 3 and 2 | A stops at floor 2 then stays in emergency |  |  |  |
| T36 | Emergency | Queue cleared | Start A 1→3, queue floor 1, then press Emergency | After the next-floor stop, A does not go on to the queued floor |  |  |  |
| T37 | Emergency | Other car used | Put A in emergency. Press hall Floor 3 | Controller sends B; A does not move |  |  |  |
| T38 | Emergency | Both cars | Emergency on A and B, then hall Floor 2 | Message “No elevator available”; no new request queued |  |  |  |
| T39 | Emergency | Reset | After emergency on A, press Reset A | A idle at current floor; normal colours; panel buttons work again |  |  |  |
| T40 | Emergency | Press twice | Press Emergency A twice while idle, and twice while moving | No crash; A stays in emergency; B still works |  |  |  |
| T41 | Emergency | Log rows | Emergency then Reset on A, click Log | Grid shows Status Emergency (press and stop) and Reset; Source Emergency Button A / Reset Button A |  |  |  |
| T42 | Doors | Open/close at floor 1 | Idle at 1, press panel 1 | Doors slide open (~1 s), stay open (~3 s), close (~1 s) only on floor 1 |  |  |  |
| T43 | Doors | Open/close at floor 2 | Send A to 2, watch arrival | Doors animate only on floor 2; floors 1 and 3 stay closed |  |  |  |
| T44 | Doors | Open/close at floor 3 | Send A to 3, watch arrival | Doors animate only on floor 3 |  |  |  |
| T45 | Doors | Never open while moving | Send A 1→3 | Door leaves stay shut until the car Y matches floor 3 |  |  |  |
| T46 | Panel | Door Open / Close | At floor 1 press Open, then Close | Open keeps/re-opens doors; Close skips the rest of the dwell |  |  |  |
| T47 | Panel | Door buttons while moving | While A is moving, press Open and Close | Status “Doors locked while moving”; car keeps going |  |  |  |
| T48 | Panel | Alarm | Press Alarm on car A | Message “Alarm pressed in Elevator A”; Log Status Alarm |  |  |  |
| T49 | Panel | Amber lights | Press 2 and 3 on panel A | Buttons glow amber; 2 goes out when A arrives at 2 |  |  |  |
| T50 | Hall | Up / Down layout | Look beside the doors | Floor 1 Up only, Floor 2 Up and Down, Floor 3 Down only |  |  |  |
| T51 | Hall | Hall call assignment | Press Floor 2 Up | Controller still picks shortest wait; hall button amber until a car arrives |  |  |  |
| T52 | Doors | Same-floor reopens | While doors are closing on 1, press 1 again | Doors open again and dwell restarts |  |  |  |
| T53 | Doors | Rapid spam | Mash floor, door and hall buttons | No crash; still one Timer per car; extra floors queued |  |  |  |
| T54 | DB | Refresh button | Click **Refresh** after making several elevator trips | DataGridView updates with the latest log rows via BackgroundWorker without freezing UI |  |  |  |
| T55 | DB | Clear button | Click **Clear Log**, confirm Yes | All database records deleted via `DatabaseHelper.ClearLog()`; DataGridView shows 0 rows; success message shown |  |  |  |
| T56 | Doors | Sequences on Elevator A | Run 1→2, 2→3, 3→1 on Car A | Doors visibly open, dwell, and close at each destination floor; other floors remain shut |  |  |  |
| T57 | Doors | Sequences on Elevator B | Run 1→3, 3→2, 2→1 on Car B | Doors visibly open, dwell, and close at each destination floor; other floors remain shut |  |  |  |
| T58 | Doors | Landing doors closed on other floors | Car A stopped at 2 with doors open | Landing doors on floors 1 and 3 stay completely closed |  |  |  |
| T59 | GUI | Header labels alignment & colors | Look at roof of BuildingView | "Shaft A" centered above Shaft A in cyan-blue; "Shaft B" centered above Shaft B in amber-gold |  |  |  |
| T60 | Passenger | Appearance on Hall Call A | Press Floor 2 Up (assigns Elevator A) | Boy spawns at Left edge of floor 2, walks with animated legs to Shaft A doors |  |  |  |
| T61 | Passenger | Appearance on Hall Call B | Press Floor 3 Down (assigns Elevator B) | Boy spawns at Right edge of floor 3, walks with animated legs to Shaft B doors |  |  |  |
| T62 | Passenger | Waiting while car travels | Press Floor 3 Down while Car B is at floor 1 | Boy walks to doors of B and waits patiently; doors do not open until Car B arrives |  |  |  |
| T63 | Passenger | Boarding when doors open | Elevator arrives, doors fully open | Boy walks inside the car cabin; status shows "Passenger inside - choose a floor" |  |  |  |
| T64 | Passenger | Doors never close on passenger | Watch doors while boy is in doorway boarding/exiting | Door dwell holds at open until the boy has cleared the doorway |  |  |  |
| T65 | Passenger | Riding inside the car | Car moves from floor 1 to floor 3 | Boy is drawn inside the car, moving vertically with CarY |  |  |  |
| T66 | Passenger | Exiting at destination | Car reaches destination floor and doors open | Boy walks out of the car, off the building edge, and is removed (Gone); doors close after he clears doorway |  |  |  |
| T67 | Passenger | Full journey 1→3 | Hall call at 1, boards Car A, press 3 on Panel A | Spawns, walks to doors, boards, rides 1→3, exits off left edge; logged in DB |  |  |  |
| T68 | Passenger | Full journey 3→1 | Hall call at 3, boards Car B, press 1 on Panel B | Spawns, walks to doors, boards, rides 3→1, exits off right edge; logged in DB |  |  |  |
| T69 | Passenger | Two passengers on different elevators | Call Floor 1 Up and Floor 3 Down | Passenger for A on Floor 1, Passenger for B on Floor 3; independent walking and boarding |  |  |  |
| T70 | Passenger | Passenger events logging | After passenger trips, click Log / Refresh | `PassengerBoarded` and `PassengerExited` rows recorded in database |  |  |  |
