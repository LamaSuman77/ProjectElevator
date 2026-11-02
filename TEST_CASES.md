# Test cases — Elevator Control System

Fill **Actual result**, **Pass/Fail** and **Screenshot ref** during your own run.

| Test ID | Task | Description | Steps | Expected result | Actual result | Pass/Fail | Screenshot ref |
| --- | --- | --- | --- | --- | --- | --- | --- |
| T01 | GUI | Main window layout | Start the app | Hall buttons, two shafts, two control panels, three floor lights per car, Log button, grid, status labels |  |  |  |
| T02 | GUI | Control names | Open Form1.Designer in Visual Studio | Names such as `btnRequestFloor1`, `btnPanelAFloor2`, `lblCurrentFloorA` |  |  |  |
| T03 | GUI | Floor displays highlight | Leave cars on floor 1 | Floor 1 labels for A and B are yellow; 2 and 3 are white |  |  |  |
| T04 | Logic | Panel A 1→2 | On panel A press 2 | Car A moves up; status `Moving: Floor 1 -> Floor 2 (Up)`; display 1 then 2 |  |  |  |
| T05 | Logic | Panel A 1→3 | Reset A to 1, press 3 | Status `Moving: Floor 1 -> Floor 3 (Up)`; floor number 1→2→3 |  |  |  |
| T06 | Logic | Panel A 2→1 | Take A to 2, press 1 | Status `Moving: Floor 2 -> Floor 1 (Down)` |  |  |  |
| T07 | Logic | Panel A 2→3 | Take A to 2, press 3 | Status `Moving: Floor 2 -> Floor 3 (Up)` |  |  |  |
| T08 | Logic | Panel A 3→1 | Take A to 3, press 1 | Status `Moving: Floor 3 -> Floor 1 (Down)`; passes 2 |  |  |  |
| T09 | Logic | Panel A 3→2 | Take A to 3, press 2 | Status `Moving: Floor 3 -> Floor 2 (Down)` |  |  |  |
| T10 | Logic | Same-floor press | While idle at 1, press 1 | No movement; status contains “already here” |  |  |  |
| T11 | Logic | Button spam while moving | Start 1→3, mash 2, 3, 1, 2 | No crash; extra floors queued; current trip not cancelled |  |  |  |
| T12 | Hall | Outside buttons | Press Floor 3 then Floor 1 in the hall | Controller assigns a car; both cars can be used |  |  |  |
| T13 | Animation | Car moves in the shaft | Watch shaft A on a 1→3 trip | Blue car slides to each floor Y; timer based, not a jump |  |  |  |
| T14 | Animation | Arrows | During up / down / idle | Panel shows ▲, ▼, or - ; floor lights match |  |  |  |
| T15 | DB | Connect on start | Run with LocalDB available | No error box; `Data\ElevatorLog.mdf` appears next to the exe |  |  |  |
| T16 | DB | Store rows | Make one trip, wait 1 second | Table has Request, Moving, Arrived; ElevatorId 1 or 2 |  |  |  |
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
| T28 | Observer | Displays follow events | Move A 1→3 | All three A floor labels and panel A update together |  |  |  |
| T29 | Images | Pictures on both cars | Start the app with ElevatorA.png and ElevatorB.png in Images | Both cars show the pictures (no A/B letter) |  |  |  |
| T30 | Images | Pictures move | Send A to floor 3 and B to floor 2 | Pictures slide with the cars; size and path match the old squares |  |  |  |
| T31 | Images | Missing picture file | Rename or remove one png, run again | One friendly MessageBox; that car falls back to the coloured square with the letter |  |  |  |
| T32 | Images | Relative path | Copy the output folder to another drive and run the exe | Pictures still load from Images next to the exe; no C:\ path in code |  |  |  |
| T33 | Emergency | Idle | Leave A idle at 1, press Emergency A | A stops at once; red text EMERGENCY - stopped at Floor 1; panel shows E |  |  |  |
| T34 | Emergency | Moving up | Send A 1→3, press Emergency between 1 and 2 | A continues to floor 2 then stops; does not stop between floors |  |  |  |
| T35 | Emergency | Moving down | Send A 3→1, press Emergency between 3 and 2 | A stops at floor 2 then stays in emergency |  |  |  |
| T36 | Emergency | Queue cleared | Start A 1→3, queue floor 1, then press Emergency | After the next-floor stop, A does not go on to the queued floor |  |  |  |
| T37 | Emergency | Other car used | Put A in emergency. Press hall Floor 3 | Controller sends B; A does not move |  |  |  |
| T38 | Emergency | Both cars | Emergency on A and B, then hall Floor 2 | Message “No elevator available”; no new request queued |  |  |  |
| T39 | Emergency | Reset | After emergency on A, press Reset A | A idle at current floor; normal colours; panel buttons work again |  |  |  |
| T40 | Emergency | Press twice | Press Emergency A twice while idle, and twice while moving | No crash; A stays in emergency; B still works |  |  |  |
| T41 | Emergency | Log rows | Emergency then Reset on A, click Log | Grid shows Status Emergency (press and stop) and Reset; Source Emergency Button A / Reset Button A |  |  |  |
