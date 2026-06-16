import { createReducer, on } from "@ngrx/store";
import { decreaseCounter, increaseCounter } from "./counter.action";

const intialStage=0;
export const counterReducer=createReducer(
    intialStage,
    on(increaseCounter,(state)=>state+1),
    on(decreaseCounter,(state)=>state-1)
)