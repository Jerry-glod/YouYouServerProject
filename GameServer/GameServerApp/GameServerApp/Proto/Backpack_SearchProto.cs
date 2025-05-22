//***********************************************************
// 描述：
// 作者：郭金宝 
// 创建时间：2025-05-21 17:34:21
// 备注：
//***********************************************************
//===================================================
//作    者：边涯  http://www.u3dol.com  QQ群：87481002
//创建时间：2018-02-25 22:40:38
//备    注：
//===================================================
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 客户端发送查询背包项消息
/// </summary>
public struct Backpack_SearchProto : IProto
{
    public ushort ProtoCode { get { return 16004; } }


    public byte[] ToArray()
    {
        using (MMO_MemoryStream ms = new MMO_MemoryStream())
        {
            ms.WriteUShort(ProtoCode);
            return ms.ToArray();
        }
    }

    public static Backpack_SearchProto GetProto(byte[] buffer)
    {
        Backpack_SearchProto proto = new Backpack_SearchProto();
        using (MMO_MemoryStream ms = new MMO_MemoryStream(buffer))
        {
        }
        return proto;
    }
}