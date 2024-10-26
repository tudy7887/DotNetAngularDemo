import { Photo } from "./photo"

export interface Member {
    id: number
    userName: string
    age: number
    photoUrl: string
    knownAs: string
    gender: string
    city: string
    country: string
    created: Date
    lastActive: Date
    introduction: string
    interests: string
    lookingFor: string 
    photos: Photo[]
}