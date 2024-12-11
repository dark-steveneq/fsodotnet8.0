using System;
using FSO.Files.HIT;

namespace FSO.HIT
{
    /// <summary>
    /// Interpreter for HIT scripts
    /// </summary>
    public class HITInterpreter
    {
        /// <summary>
        /// List of all valid instructions
        /// </summary>
        public static HITInstruction[] Instructions = [
            NOP,            Note,           NoteOn,         NoteOff,        LoadB,          LoadL,          Set,            Call, 
            Return,         Wait,           CallEntryPoint, WaitSamp,       End,            Jump,           Test,           NOP, 
            Add,            Sub,            Div,            Mul,            Cmp,            Less,           Greater,        Not, 
            Rand,           Abs,            Limit,          Error,          Assert,         AddToGroup,     RemoveFromGroup, GetVar, 
            Loop,           SetLoop,        Callback,       SmartAdd,       SmartRemove,    SmartRemoveAll, SmartSetCrit,   SmartChoose, 
            And,            NAnd,           Or,             NOr,            XOr,            Max,            Min,            Inc, 
            Dec,            PrintReg,       PlayTrack,      KillTrack,      Push,           PushMask,       PushVars,       CallMask, 
            CallPush,       Pop,            Test1,          Test2,          Test3,          Test4,          IfEqual,        IfNotEqual,
            IfGreater,      IfLess,         IfGreatOrEq,    IfLessOrEq,     SmartSetList,   SeqGroupKill,   SeqGroupWait,   SeqGroupReturn,
            GetSrcDataField, SeqGroupTrackID, SetLL,        SetLT,          SetTL,          WaitEqual,      WaitNotEqual,   WaitGreater,
            WaitLess,       WaitGreatOrEq,  WaitLessOrEq,   Duck,           Unduck,         TestX,          SetLG,          SetGL,
            Throw,          SetSrcDataField, StopTrack,     SetChanReg,     PlayNote,       StopNote,       KillNote,       SmartIndex,
            NoteOnLoop
        ];

        /// <summary>
        /// NOP instruction, doesn't do anything
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult NOP(HITThread thread)
        {
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Note instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Note(HITThread thread)
        {
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Play a note, whose ID resides in the specified variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult NoteOn(HITThread thread)
        {
            var dest = thread.ReadByte();

            thread.WriteVar(dest, thread.NoteOn());
            
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// NoteOff instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult NoteOff(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// LoadB instruction, sign-extends a 1-byte constant to 4 bytes and write to a variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult LoadB(HITThread thread)
        {
            var dest = thread.ReadByte();
            var value = (sbyte)thread.ReadByte();

            thread.WriteVar(dest, value);

            thread.SetFlags(value);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// LoadL instruction, writes a 4-byte constant to a variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult LoadL(HITThread thread)
        {
            var dest = thread.ReadByte();
            var value = thread.ReadInt32();

            thread.WriteVar(dest, value);
            thread.SetFlags(value);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Set instruction, copies the contents of one variable into another
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Set(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadVar(thread.ReadByte());

            thread.WriteVar(dest, src);

            thread.SetFlags(src);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Remapps program counter from call ID to function offset if applicable (TS1)
        /// </summary>
        /// <param name="id">ID to remap</param>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>New program counter</returns>
        public static uint PCTrans(uint id, HITThread thread)
        {
            if (Content.Content.Get().TS1)
            {
                //dunno why this is ts1 specific, but whatever
                //need to remap from the call id to the function offset

                thread.ResGroup.hot.TrackData.TryGetValue(id, out id);
            }
            return id;
        }

        /// <summary>
        /// Call instruction, pushes the instruction pointer and jumps to the given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Call(HITThread thread)
        {
            uint targ = thread.ReadUInt32();

            thread.Stack.Push((int)thread.PC);
            thread.PC = PCTrans(targ, thread);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Return instruction, stops execution
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Kill result</returns>
        public static HITResult Return(HITThread thread)
        {
            return HITResult.KILL;
        }

        /// <summary>
        /// Wait instruction, waits for a length of time in milliseconds, specified by a variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue or halt result</returns>
        public static HITResult Wait(HITThread thread)
        {
            var src = thread.ReadByte();
            if (thread.WaitRemain == -1) thread.WaitRemain = thread.ReadVar(src);
            thread.WaitRemain -= 16; //assuming tick rate is 60 times a second
            if (thread.WaitRemain > 0)
            {
                thread.PC -= 2;
                return HITResult.HALT;
            }
            else
            {
                thread.WaitRemain = -1;
                return HITResult.CONTINUE;
            }
        }

        /// <summary>
        /// CallEntryPoint instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult CallEntryPoint(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// WaitSamp instruction, waits for the previously selected note to finish playing
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Halt result</returns>
        public static HITResult WaitSamp(HITThread thread)
        {
            if (thread.NoteActive(thread.LastNote))
            {
                thread.PC--;
                return HITResult.HALT;
            }
            else
                return HITResult.HALT;
        }

        /// <summary>
        /// End instruction, pops the instruction pointer from the stack and jumps
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue or kill result</returns>
        public static HITResult End(HITThread thread)
        {
            if (thread.Stack.Count > 0)
            {
                thread.PC = (uint)thread.Stack.Pop();
                return HITResult.CONTINUE;
            }
            else
            {
                if (thread.Loop && thread.HasSetLoop)
                {
                    return Loop(thread);
                } else return HITResult.KILL;
            }
        }

        /// <summary>
        /// Jump instruction, jumps to a given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Jump(HITThread thread)
        {
            var read = thread.ReadByte();

            if (read > 15) //literal
            {
                thread.PC--; //backtraaackkk
                thread.PC = PCTrans(thread.ReadUInt32(), thread);
            }
            else //no idea if there are collisions. if there are i'm blaming fatbag. >:)
            {
                thread.PC = PCTrans((uint)thread.ReadVar(read), thread);
                if (thread.ReadByte() == 0)
                    thread.PC += 2; //if next is no-op, the operand is 4 byte
                else thread.PC--; //operand is 1 byte (next is an instruction), backtrack
            }

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Test instruction, examines a variable and sets the flags.
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Test(HITThread thread)
        {
            var value = thread.ReadVar(thread.ReadByte());

            thread.SetFlags(value);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Add instruction, increments the "dest" variable by the "src" variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Add(HITThread thread) //0x10
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            var result = thread.ReadVar(dest) + thread.ReadVar(src);
            thread.WriteVar(dest, result);

            thread.SetFlags(result);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Sub instruction, decrements the "dest" variable by the "src" variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Sub(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            var result = thread.ReadVar(dest) - thread.ReadVar(src);
            thread.WriteVar(dest, result);

            thread.SetFlags(result);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Div instruction, divides the "dest" variable by the "src" variable and truncates the result
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Div(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            var result = thread.ReadVar(dest) / thread.ReadVar(src);
            thread.WriteVar(dest, result);

            thread.SetFlags(result);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Mul instruction, multiplies the "dest" variable by the "src" variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Mul(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            var result = thread.ReadVar(dest) * thread.ReadVar(src);
            thread.WriteVar(dest, result);

            thread.SetFlags(result);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Cmp instruction, compares two variables and sets the flags
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Cmp(HITThread thread) //same as sub, but does not set afterwards.
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            var result = thread.ReadVar(dest) - thread.ReadVar(src);

            thread.SetFlags(result);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Less instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Less(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Greater instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Greater(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Not instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Not(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Rand instruction, generates a random number between "low" and "high" variables, inclusive, and stores the result in the "dest" variables
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Rand(HITThread thread)
        {
            var dest = thread.ReadByte();
            var low = thread.ReadByte();
            var high = thread.ReadByte();

            var result = (new Random()).Next(high+1-low)+low;
            thread.WriteVar(dest, result);

            thread.SetFlags(result);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Abs instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Abs(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Limit instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Limit(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Error instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Error(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Assert instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Assert(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// AddFromGroup instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult AddToGroup(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// RemoveFromGroup instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult RemoveFromGroup(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// GetVar instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult GetVar(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Loop instruction, jumps back to the loop point (start of track subroutine by default)
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Loop(HITThread thread) //0x20
        {
            thread.PC = (uint)thread.LoopPointer;
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SetLoop instruction, sets loop point to the current position
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetLoop(HITThread thread)
        {
            thread.LoopPointer = (int)thread.PC;
            thread.HasSetLoop = true;
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Callback instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Callback(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SmartAdd instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SmartAdd(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SmartAdd instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SmartRemove(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SmartRemoveAll instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SmartRemoveAll(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SmartSetCrit instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SmartSetCrit(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SmartChoose instruction, sets the specified variable to a random entry from the selected hitlist
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SmartChoose(HITThread thread)
        {
            var dest = thread.ReadByte();
            thread.WriteVar(dest, (int)thread.HitlistChoose());
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// And instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult And(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// NAnd instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult NAnd(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Or instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Or(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// NOr instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult NOr(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// XOr instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult XOr(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Max instruction, finds the higher of the "dest" variable and the "src" constant and stores the result in a variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Max(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadInt32();

            var result = Math.Max(thread.ReadVar(dest), src);
            thread.WriteVar(dest, result);

            thread.SetFlags(result);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Min instruction, finds the lower of the "dest" variable and the "src" constant and stores the result in a variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Min(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadInt32();

            var result = Math.Min(thread.ReadVar(dest), src);
            thread.WriteVar(dest, result);

            thread.SetFlags(result);
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Inc instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Inc(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Dec instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Dec(HITThread thread) //0x30
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// PrintReg instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult PrintReg(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// PlayTrack instruction, plays track with ID read from a specified variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult PlayTrack(HITThread thread)
        {
            var dest = thread.ReadByte();
            return HITResult.CONTINUE; //Not used in TSO.
        }

        /// <summary>
        /// KillTrack instruction, stop track with ID read from a specified variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult KillTrack(HITThread thread)
        {
            var src = thread.ReadByte();
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Push instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Push(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// PushMask instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult PushMask(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// PushVars instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult PushVars(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// CallMask instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult CallMask(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// CallPush instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult CallPush(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Pop instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Pop(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Test1 instruction, unused in The Sims and seemingly doesn't have a function
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Test1(HITThread thread)
        {
            //no idea what these do. examples?
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Test2 instruction, unused in The Sims and seemingly doesn't have a function
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Test2(HITThread thread)
        {
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Test3 instruction, unused in The Sims and seemingly doesn't have a function
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Test3(HITThread thread)
        {
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Test4 instruction, unused in The Sims and seemingly doesn't have a function
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Test4(HITThread thread)
        {
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// IfEqual instruction, if the zero flag is set, jumps to the given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult IfEqual(HITThread thread)
        {
            var loc = thread.ReadUInt32();

            if (thread.ZeroFlag)
                thread.PC = PCTrans(loc, thread);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// IfNotEqual instruction, if the zero flag is not set, jumps to the given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult IfNotEqual(HITThread thread)
        {
            var loc = thread.ReadUInt32();

            if (!thread.ZeroFlag)
                thread.PC = PCTrans(loc, thread);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// IfGreater instruction, if the sign flag is not set and the zero flag is not set, jumps to the given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult IfGreater(HITThread thread) //0x40
        {
            var loc = thread.ReadUInt32();

            if (!thread.SignFlag && !thread.ZeroFlag)
                thread.PC = PCTrans(loc, thread); //last set/compare result was > 0

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// IfLess instruction, if the sign flag is set, jumps to the given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult IfLess(HITThread thread)
        {
            var loc = thread.ReadUInt32();

            if (thread.SignFlag)
                thread.PC = PCTrans(loc, thread); //last set/compare result was < 0

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// IfGreatOrEq instruction, if the sign flag is not set, jumps to the given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult IfGreatOrEq(HITThread thread)
        {
            var loc = thread.ReadUInt32();

            if (!thread.SignFlag) thread.PC = PCTrans(loc, thread); //last set/compare result was >= 0

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// IfLessOrEq instruction, if the sign flag is set or the zero flag is set, jumps to the given address
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult IfLessOrEq(HITThread thread)
        {
            var loc = thread.ReadUInt32();

            if (thread.SignFlag || thread.ZeroFlag) thread.PC = PCTrans(loc, thread); //last set/compare result was <= 0

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SmartSetList instruction, chooses a global hitlist, or 0 for the one local to the track (source: defaultsyms.txt)
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SmartSetList(HITThread thread)
        { //sets the hitlist
            var src = thread.ReadByte();
            thread.LoadHitlist((uint)thread.ReadVar(src));

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SeqGroupKill instruction, kills an actor's vocals, given a constant ID.
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SeqGroupKill(HITThread thread)
        {
            var src = thread.ReadByte();

            if (src == (byte)HITPerson.Instance)
                thread.KillVocals();
            else
            {
                //TODO: Implement system for keeping track of which object created a thread
                //      and kill that thread's sounds (src == ObjectID).
            }

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SeqGroupWait instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SeqGroupWait(HITThread thread) //unused in the sims
        {
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SeqGroupReturn instruction, kill a sequence group with the return value specified by a constant... or at least it should do that but the handler isn't implemented
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SeqGroupReturn(HITThread thread)
        {
            var src = thread.ReadByte();
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// GetSrcDataField instruction, reads address "field" variable + 10010 to "dest" variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult GetSrcDataField(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();
            var field = thread.ReadByte();

            //int ObjectVar = thread.ReadVar(src); //always 12?
            int FieldVar = thread.ReadVar(field);

            int ObjectVar = thread.ReadVar(10010 + FieldVar);
            thread.WriteVar(dest, ObjectVar);
            thread.SetFlags(ObjectVar);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SeqGroupTrackID instruction, not implemented
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SeqGroupTrackID(HITThread thread)
        {
            var dest = thread.ReadByte(); //uhhhh
            var src = thread.ReadByte();
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SetLL instruction, copies the contents of one variable into another
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetLL(HITThread thread)
        {
            Set(thread);
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SetLT instruction, not implemented
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetLT(HITThread thread)
        {
            //set local... to... t... yeah i don't know either
            //might be object vars

            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SetTL instruction, not implemented and unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetTL(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// WaitEqual instruction, waits until two variables are equal
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult WaitEqual(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadByte();

            if (thread.ReadVar(dest) != thread.ReadVar(dest))
            {
                thread.PC -= 3;
                return HITResult.HALT;
            }
            else
                return HITResult.CONTINUE;
        }

        /// <summary>
        /// WaitNotEqual instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult WaitNotEqual(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// WaitGreater instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult WaitGreater(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// WaitLess instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult WaitLess(HITThread thread) //0x50
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// WaitGreatOrEq instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult WaitGreatOrEq(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// WaitLessOrEq instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult WaitLessOrEq(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// Duck instruction, silences audio with lower priority than this "Imagine all the other sounds getting quieter when the fire music plays"
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Duck(HITThread thread)
        {
            thread.Duck();
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Unduck instruction, inverse of Duck instruction
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Unduck(HITThread thread)
        {
            thread.Unduck();
            return HITResult.CONTINUE; //quack
        }

        /// <summary>
        /// TestX instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult TestX(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SetLG instruction, read local and write to global
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetLG(HITThread thread)
        {
            var local = thread.ReadByte();
            var global = thread.ReadInt32();

            thread.VM.WriteGlobal(global, local);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SetGL instruction, read global and write to local
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetGL(HITThread thread)
        {
            var dest = thread.ReadByte();
            var src = thread.ReadInt32();

            int Global = thread.VM.ReadGlobal(src);
            thread.WriteVar(dest, Global);

            return HITResult.CONTINUE;
        }

        /// <summary>
        /// Throw instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult Throw(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SetSrcDataField instruction, not implemented
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetSrcDataField(HITThread thread)
        {
            var value = thread.ReadByte();
            var src = thread.ReadByte();
            var field = thread.ReadByte();

            //TODO: System for keeping track of which objects correspond to ObjectID.

            return HITResult.CONTINUE; //you can set these??? what
        }

        /// <summary>
        /// StopTrack instruction, stops playing a track with specified ID, not implemented
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult StopTrack(HITThread thread)
        {
            var src = thread.ReadByte();
            return HITResult.CONTINUE;
        }

        /// <summary>
        /// SetChanReg instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SetChanReg(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// PlayNote instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult PlayNote(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// StopNote instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult StopNote(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// KillNote instruction, unused in The Sims
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult KillNote(HITThread thread)
        {
            return HITResult.CONTINUE; //unused in the sims
        }

        /// <summary>
        /// SmartIndex instruction, loads a track from a hitlist by index
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Continue result</returns>
        public static HITResult SmartIndex(HITThread thread)
        {
            var destVar = thread.ReadByte();
            //var dest = thread.ReadVar(thread.ReadByte());
            var index = thread.ReadVar(thread.ReadByte());

            //thread.LoadHitlist((byte)dest);
            int TrackID = (int)thread.LoadTrack(index);

            thread.WriteVar(destVar, TrackID);

            return HITResult.CONTINUE; //Appears to be unused.
        }

        /// <summary>
        /// NoteOnLoop instruction, writes note loop to "dest" variable
        /// </summary>
        /// <param name="thread">HITThread to affect</param>
        /// <returns>Halt result</returns>
        public static HITResult NoteOnLoop(HITThread thread) //0x60
        {
            var dest = thread.ReadByte();
            thread.WriteVar(dest, thread.NoteLoop());
            return HITResult.HALT;
        }
    }
    public delegate HITResult HITInstruction(HITThread thread);

    /// <summary>
    /// Enum with instruction results
    /// </summary>
    public enum HITResult
    {
        /// <summary>
        /// Continue execution
        /// </summary>
        CONTINUE,
        /// <summary>
        /// Halt execution
        /// </summary>
        HALT,
        /// <summary>
        /// Stop execution
        /// </summary>
        KILL
    }
}
